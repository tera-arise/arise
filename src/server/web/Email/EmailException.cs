namespace Arise.Server.Web.Email;

[SuppressMessage("", "CA1032")]
[SuppressMessage("", "CA1064")]
internal sealed class EmailException : Exception
{
    public EmailException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
