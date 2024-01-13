namespace Arise.Net;

public sealed class GameConnectionException : Exception
{
    public GameConnectionException()
        : this("An unspecified connection error occurred.")
    {
    }

    public GameConnectionException(string message)
        : base(message)
    {
    }

    public GameConnectionException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
