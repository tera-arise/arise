namespace Arise.Tools.Distributor;

internal static class ClientDistributor
{
    private sealed record class Manifest(int Revision, IDictionary<string, ManifestEntry> Entries);

    private sealed record class ManifestEntry(int Archive, string Hash);

    private const string ManifestName = "manifest.json";

    public static async ValueTask DistributeAsync(DistributorOptions options)
    {
        var ghc = new GitHubClient(
            new Octokit.ProductHeaderValue(ThisAssembly.AssemblyName, ThisAssembly.AssemblyVersion))
        {
            Credentials = new Credentials(options.Token),
        };

        var timeout = options.Timeout;

        if (timeout == TimeSpan.Zero)
            timeout = TimeSpan.FromHours(1);

        ghc.SetRequestTimeout(timeout);

        var repositoryApi = ghc.Repository;
        var releaseApi = repositoryApi.Release;

        var releaseName = $"r{options.Revision}";

        await Terminal.OutLineAsync($"Creating release {releaseName}...");

        var repository = await repositoryApi.Get(options.Owner, options.Repository);
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
                new(releaseName)
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
                .ToDictionary(file => Path.GetRelativePath(root.FullName, file.FullName), static file => file),
            StringComparer.Ordinal);
        var entries = new SortedDictionary<string, ManifestEntry>(StringComparer.Ordinal);

        for (var i = 0; files.Count != 0; i++)
        {
            await using var stream = new MemoryStream();

            var zipName = $"TERA.EU.{options.Revision}.{i:00}.zip";

            await Terminal.OutLineAsync($"Packing '{zipName}' in memory...");

            using (var zip = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
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

            _ = await releaseApi.UploadAsset(release, new(zipName, MediaTypeNames.Application.Zip, stream, timeout));
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
                release, new(ManifestName, MediaTypeNames.Application.Json, stream, timeout));
        }
    }
}
