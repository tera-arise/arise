namespace Arise.Server.Web.Mail;

public sealed class MailSender
{
    private readonly ISendGridClient _client;

    private readonly IOptionsMonitor<WebOptions> _options;

    public MailSender(ISendGridClient client, IOptionsMonitor<WebOptions> options)
    {
        _client = client;
        _options = options;
    }

    public async ValueTask SendAsync(
        string receiver, string subject, string content, CancellationToken cancellationToken)
    {
        // TODO: Add logging.

        var message = new SendGridMessage
        {
            From = new(_options.CurrentValue.MailAddress, "TERA Arise"),
            Subject = $"{subject} | TERA Arise",
            PlainTextContent = $"""
            Hi!

            {content}

            Regards,
            TERA Arise Team
            """.ReplaceLineEndings("\r\n"), // Emails use CRLF.
        };

        message.AddTo(receiver);

        try
        {
            _ = await _client.SendEmailAsync(message, cancellationToken);
        }
        catch (HttpRequestException e)
        {
            throw new MailException("A low-level connection error occurred.", e);
        }
        catch (TaskCanceledException e)
        {
            throw new MailException("A connection timeout occurred.", e);
        }
        catch (RequestErrorException e)
        {
            throw new MailException("A SendGrid request error occurred.", e);
        }
        catch (SendGridInternalException e)
        {
            throw new MailException("An internal SendGrid error occurred.", e);
        }
    }
}
