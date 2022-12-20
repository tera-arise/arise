namespace Arise.Server.Storage;

public sealed class StorageOptions : IOptions<StorageOptions>
{
    public string ConnectionString { get; set; } = "Host=localhost; Username=arise; Password=arise; Database=arise";

    public int MaxRetryCount { get; set; } = 5;

    StorageOptions IOptions<StorageOptions>.Value => this;
}
