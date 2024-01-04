namespace Arise.Tools.Distributor;

internal static class ClientDistributor
{
    private sealed record class Manifest(int Revision, IDictionary<string, ManifestEntry> Entries);

    private sealed record class ManifestEntry(int Archive, string Hash);

    private const string ManifestName = "manifest.json";

    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
    };

    public static async ValueTask DistributeAsync(DistributorOptions options)
    {
        using var client = ClientFactory.Create();
        using var adapter = RequestAdapter.Create(
            new TokenAuthenticationProvider(
                $"{ThisAssembly.AssemblyName} {ThisAssembly.AssemblyVersion}", options.GitHubToken),
            client);

        var timeout = options.UploadTimeout;

        if (timeout == TimeSpan.Zero)
            timeout = TimeSpan.FromHours(1);

        client.Timeout = timeout;

        var ghc = new GitHubClient(adapter);

        var repoBuilder = ghc.Repos[options.RepositoryOwner][options.RepositoryName];
        var releaseBuilder = repoBuilder.Releases;

        var releaseName = $"r{options.TeraRevision}";

        await Terminal.OutLineAsync($"Creating release {releaseName}...");

        var root = options.TeraDirectory;

        var releaseWithTagBuilder = releaseBuilder.Tags[releaseName];
        var release = await releaseWithTagBuilder.GetAsync();

        if (release == null)
        {
            release = (await releaseBuilder.PostAsync(new()
            {
                TagName = releaseName,
                Body =
                    $"""
                    ```text
                    {(await File.ReadAllTextAsync(
                        Path.Combine(root.FullName, "Client", "Binaries", "ReleaseRevision.txt"))).Trim()}
                    ```
                    """,
                TargetCommitish = "7c680c08e868e8365eb0b2eddf660155591f15f4",
                Draft = true,
            }))!;

            await Terminal.OutLineAsync($"Draft release created at: {release.HtmlUrl}");
        }
        else
            await Terminal.OutLineAsync($"Existing release found at: {release.HtmlUrl}");

        await Terminal.OutLineAsync($"Deleting {release.Assets!.Count} old assets...");

        foreach (var asset in release.Assets)
            await releaseBuilder.Assets[(int)asset.Id!].DeleteAsync();

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

            var zipName = $"TERA.EU.{options.TeraRevision}.{i:00}.zip";

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

            // TODO: https://github.com/octokit/dotnet-sdk/issues/27
            _ = await releases.UploadAsset(release, new(zipName, MediaTypeNames.Application.Zip, stream, timeout));
        }

        {
            await using var stream = new MemoryStream();

            await Terminal.OutLineAsync($"Serializing '{ManifestName}' in memory...");

            await JsonSerializer.SerializeAsync(stream, new Manifest(options.TeraRevision, entries), _jsonOptions);

            stream.Position = 0;

            await Terminal.OutLineAsync(
                $"Uploading '{ManifestName}' ({stream.Length} bytes) to release {releaseName}...");

            // TODO: https://github.com/octokit/dotnet-sdk/issues/27
            _ = await releases.UploadAsset(
                release, new(ManifestName, MediaTypeNames.Application.Json, stream, timeout));
        }
    }
}
