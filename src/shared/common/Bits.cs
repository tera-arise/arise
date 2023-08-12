namespace Arise;

public static class Bits
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Join<T>(ReadOnlySpan<(T Value, int Start, int Count)> fields)
        where T : IBinaryInteger<T>
    {
        var result = T.Zero;

        foreach (var (value, start, count) in fields)
            result = Insert(result, value, start, count);

        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Extract<T>(T value, int start, int count)
        where T : IBinaryInteger<T>
    {
        return value >>> start & (T.One << count) - T.One;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Insert<T>(T value1, T value2, int start, int count)
        where T : IBinaryInteger<T>
    {
        var mask = (T.One << count) - T.One;

        return value1 & ~(mask << start) | (value2 & mask) << start;
    }
}
