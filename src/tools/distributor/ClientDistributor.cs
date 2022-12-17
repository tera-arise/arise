namespace Arise.Tools.Distributor;

internal static class ClientDistributor
{
    private sealed record class Manifest(int Revision, IDictionary<string, ManifestEntry> Entries);

    private sealed record class ManifestEntry(int Archive, string Hash);

    private const string ManifestName = "manifest.json";

    public static async Task DistributeAsync(DistributorOptions options)
    {
        var ghc = new GitHubClient(new ProductHeaderValue(ThisAssembly.AssemblyName, ThisAssembly.AssemblyVersion))
        {
            Credentials = new Credentials(options.Token),
        };

        ghc.SetRequestTimeout(options.Timeout);

        var repositoryApi = ghc.Repository;
        var releaseApi = repositoryApi.Release;

        var releaseName = $"r{options.Revision}";

        await Terminal.OutLineAsync($"Creating release {releaseName}...");

        var repository = await repositoryApi.Get("tera-arise", "arise-client");
        var root = options.Directory;

        Release release;

        try
        {
            release = await releaseApi.Get(repository.Id, releaseName);

            await Terminal.OutLineAsync($"Existing release found at: {release.HtmlUrl}");
        }
        catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            release = await releaseApi.Create(
                repository.Id,
                new NewRelease(releaseName)
                {
                    Name = releaseName,
                    Body =
                        $"""
                        ```text
                        {(await File.ReadAllTextAsync(
                            Path.Combine(root.FullName, "Client", "Binaries", "ReleaseRevision.txt"))).Trim()}
                        ```
                        """,
                    TargetCommitish = "7c680c08e868e8365eb0b2eddf660155591f15f4",
                    Draft = true,
                });

            await Terminal.OutLineAsync($"Draft release created at: {release.HtmlUrl}");
        }

        await Terminal.OutLineAsync($"Deleting {release.Assets.Count} old assets...");

        foreach (var asset in release.Assets)
            await releaseApi.DeleteAsset(repository.Id, asset.Id);

        await Terminal.OutLineAsync($"Gathering manifest files in '{root}'...");

        var files = new SortedDictionary<string, FileInfo>(
            root
                .EnumerateFiles("*", SearchOption.AllDirectories)
                .ToDictionary(file => Path.GetRelativePath(root.FullName, file.FullName), file => file),
            StringComparer.Ordinal);
        var entries = new SortedDictionary<string, ManifestEntry>(StringComparer.Ordinal);

        for (var i = 0; files.Count != 0; i++)
        {
            await using var stream = new MemoryStream();

            var zipName = $"TERA.EU.{options.Revision}.{i:00}.zip";

            await Terminal.OutLineAsync($"Packing '{zipName}' in memory...");

            using (var zip = new ZipArchive(stream, ZipArchiveMode.Create, true))
            {
                var size = 0;

                while (files.FirstOrDefault(kvp => size + kvp.Value.Length <= int.MaxValue) is
                    {
                        Key: string path,
                        Value: var file,
                    })
                {
                    await Terminal.OutLineAsync($"  {path}");

                    _ = files.Remove(path);

                    await using var inStream = file.OpenRead();
                    await using var zipStream = zip.CreateEntry(path, CompressionLevel.SmallestSize).Open();

                    await inStream.CopyToAsync(zipStream);

                    inStream.Position = 0;

                    entries.Add(path, new(i, Convert.ToHexString(await SHA256.HashDataAsync(inStream))));

                    size += (int)file.Length;
                }
            }

            stream.Position = 0;

            await Terminal.OutLineAsync($"Uploading '{zipName}' ({stream.Length} bytes) to release {releaseName}...");

            _ = await releaseApi.UploadAsset(
                release, new ReleaseAssetUpload(zipName, MediaTypeNames.Application.Zip, stream, options.Timeout));
        }

        {
            await using var stream = new MemoryStream();

            await Terminal.OutLineAsync($"Serializing '{ManifestName}' in memory...");

            await JsonSerializer.SerializeAsync(
                stream,
                new Manifest(options.Revision, entries),
                new JsonSerializerOptions(JsonSerializerDefaults.Web)
                {
                    WriteIndented = true,
                });

            stream.Position = 0;

            await Terminal.OutLineAsync(
                $"Uploading '{ManifestName}' ({stream.Length} bytes) to release {releaseName}...");

            _ = await releaseApi.UploadAsset(
                release,
                new ReleaseAssetUpload(ManifestName, MediaTypeNames.Application.Json, stream, options.Timeout));
        }
    }
}
