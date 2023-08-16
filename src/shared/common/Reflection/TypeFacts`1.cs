namespace Arise.Reflection;

public static class TypeFacts<T>
{
    // This class exploits the JIT's ability to optimize based on static readonly fields in higher tiers.

    public static bool IsFlags { get; } = typeof(T).GetCustomAttribute<FlagsAttribute>() != null;
}
