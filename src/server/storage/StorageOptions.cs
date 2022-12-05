namespace Arise.Server.Storage;

public sealed class StorageOptions : IOptions<StorageOptions>
{
    public string ConnectionString { get; set; } = "Host=127.0.0.1; Username=arise";

    public int MaxRetryCount { get; set; } = 5;

    StorageOptions IOptions<StorageOptions>.Value => this;
}
