namespace Arise.Server.Web.Email;

public sealed class EmailException : Exception
{
    public EmailException()
        : this("An unknown mail error occurred.")
    {
    }

    public EmailException(string? message)
        : base(message)
    {
    }

    public EmailException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
