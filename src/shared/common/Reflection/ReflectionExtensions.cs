// SPDX-License-Identifier: AGPL-3.0-or-later

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

    public static bool TryGetMetadata(this Assembly assembly, string key, [MaybeNullWhen(false)] out string value)
    {
        value = assembly
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .SingleOrDefault(attr => attr.Key == key)
            ?.Value;

        return value != null;
    }
}
