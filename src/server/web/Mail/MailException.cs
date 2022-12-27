namespace Arise.Server.Web.Mail;

public sealed class MailException : Exception
{
    public MailException()
        : this("An unknown mail error occurred.")
    {
    }

    public MailException(string? message)
        : base(message)
    {
    }

    public MailException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
