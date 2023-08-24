namespace Arise.Diagnostics;

[SuppressMessage("", "CA1815")]
public readonly struct SlimStopwatch
{
    public TimeSpan Elapsed => Stopwatch.GetElapsedTime(_timestamp);

    private readonly long _timestamp;

    private SlimStopwatch(long timestamp)
    {
        _timestamp = timestamp;
    }

    public static SlimStopwatch Create()
    {
        return new(Stopwatch.GetTimestamp());
    }
}
