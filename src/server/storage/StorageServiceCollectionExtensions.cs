using Arise.Server.Storage.Documents;

namespace Arise.Server.Storage;

public static class StorageServiceCollectionExtensions
{
    public static IServiceCollection AddStorageServices(this IServiceCollection services)
    {
        return services
            .AddOptions<StorageOptions>()
            .BindConfiguration("Storage")
            .Services
            .AddSingleton<IClock>(SystemClock.Instance)
            .AddSingleton(DateTimeZoneProviders.Tzdb)
            .AddMarten(provider =>
            {
                var store = new StoreOptions
                {
                    SourceCodeWritingEnabled = false,
                    DatabaseSchemaName = provider.GetRequiredService<IHostEnvironment>().EnvironmentName.ToLower(),
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
                        .Where(type => type.GetInterfaces().Contains(typeof(IDocument))));
                _ = store.Policies.ForAllDocuments(mapping =>
                    {
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
            .Services; // TODO: https://github.com/JasperFx/marten/issues/2414
    }
}
