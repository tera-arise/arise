using Arise.Server.Storage.Documents;

namespace Arise.Server.Storage;

public static class StorageServiceCollectionExtensions
{
    [SuppressMessage("", "CA1308")]
    public static IServiceCollection AddStorageServices(this IServiceCollection services)
    {
        return services
            .AddSingleton<IClock>(SystemClock.Instance)
            .AddMarten(static provider =>
            {
                var store = new StoreOptions
                {
                    SourceCodeWritingEnabled = false,
                    DatabaseSchemaName =
                        provider.GetRequiredService<IHostEnvironment>().EnvironmentName.ToLowerInvariant(),
                    AutoCreateSchemaObjects = AutoCreate.CreateOrUpdate,
                };

                store.UseDefaultSerialization(
                    enumStorage: EnumStorage.AsString,
                    casing: Casing.SnakeCase,
                    serializerType: SerializerType.SystemTextJson);
                store.UseNodaTime();

                store.RegisterDocumentTypes(
                    typeof(ThisAssembly)
                        .Assembly
                        .ExportedTypes
                        .Where(static type => type.GetInterfaces().Contains(typeof(IDocument))));
                _ = store.Policies.ForAllDocuments(static mapping =>
                    {
                        mapping.Metadata.CreatedAt.Enabled = true;

                        if (mapping.DocumentType.Assembly == typeof(ThisAssembly).Assembly)
                            mapping.Alias = mapping.Alias[..^"Document".Length];
                    });

                var options = provider.GetRequiredService<IOptions<StorageOptions>>().Value;

                store.Connection(options.ConnectionString);
                store.RetryPolicy(DefaultRetryPolicy.Times(options.MaxRetryCount));

                return store;
            })
            .UseLightweightSessions()
            .ApplyAllDatabaseChangesOnStartup()
            .AssertDatabaseMatchesConfigurationOnStartup()
            .Services
            .AddAriseServerStorage();
    }
}
