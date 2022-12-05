using Arise.Server.Storage.Documents;

namespace Arise.Server.Storage;

public static class StorageServiceCollectionExtensions
{
    public static IServiceCollection AddStorageServices(this IServiceCollection services, HostBuilderContext context)
    {
        var options = new StorageOptions();

        context.Configuration.GetSection("Storage").Bind(options);

        return services
            .AddSingleton<IClock>(SystemClock.Instance)
            .AddSingleton(DateTimeZoneProviders.Tzdb)
            .AddMarten(opts =>
            {
                opts.SourceCodeWritingEnabled = false;
                opts.DatabaseSchemaName = context.HostingEnvironment.EnvironmentName.ToLower();
                opts.AutoCreateSchemaObjects = AutoCreate.CreateOrUpdate;

                opts.UseDefaultSerialization(
                    casing: Casing.SnakeCase,
                    collectionStorage: CollectionStorage.AsArray,
                    serializerType: SerializerType.SystemTextJson);
                opts.UseNodaTime();

                opts.RegisterDocumentTypes(
                    typeof(ThisAssembly)
                        .Assembly
                        .ExportedTypes
                        .Where(type => type.GetInterfaces().Contains(typeof(IDocument))));
                _ = opts
                    .Policies
                    .ForAllDocuments(mapping =>
                    {
                        if (mapping.DocumentType.Assembly == typeof(ThisAssembly).Assembly)
                            mapping.Alias = mapping.Alias[..^8];
                    });

                opts.Connection(options.ConnectionString);
                opts.RetryPolicy(DefaultRetryPolicy.Times(options.MaxRetryCount));
            })
            .UseLightweightSessions()
            .ApplyAllDatabaseChangesOnStartup()
            .Services; // TODO: https://github.com/JasperFx/marten/issues/2414
    }
}
