// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Arise.Server.Storage;

internal sealed class StorageOptions : IOptions<StorageOptions>
{
    public string ConnectionString { get; set; } = "Host=localhost; Username=arise; Password=arise; Database=arise";

    StorageOptions IOptions<StorageOptions>.Value => this;

    [RegisterServices]
    public static void Register(IServiceCollection services)
    {
        _ = services
            .AddOptions<StorageOptions>()
            .BindConfiguration("Storage");
    }
}
