namespace Arise.Diagnostics;

[SuppressMessage("", "CA1032")]
[SuppressMessage("", "CA1064")]
internal sealed class AssertionException : Exception
{
    public AssertionException(string expression)
        : base($"Assertion '{expression}' failed.")
    {
    }
}
