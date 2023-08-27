namespace Arise.Reflection;

public static class ReflectionExtensions
{
    public static string GetMetadata(this Assembly assembly, string key)
    {
        return assembly
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .Single(attr => attr.Key == key)
            .Value!;
    }
}
