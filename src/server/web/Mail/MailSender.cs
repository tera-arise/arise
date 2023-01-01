namespace Arise.Server.Web.Mail;

public sealed partial class MailSender
{
    private static partial class Log
    {
        [LoggerMessage(0, LogLevel.Information, "Sent {Subject} to {Receiver} in {ElapsedMs:0.0000} ms")]
        public static partial void SentMail(ILogger logger, string receiver, string subject, double elapsedMs);
    }

    private readonly IOptionsMonitor<WebOptions> _options;

    private readonly ILogger<MailSender> _logger;

    private readonly ISendGridClient _client;

    public MailSender(IOptionsMonitor<WebOptions> options, ILogger<MailSender> logger, ISendGridClient client)
    {
        _options = options;
        _logger = logger;
        _client = client;
    }

    public async ValueTask SendAsync(
        string receiver, string subject, string content, CancellationToken cancellationToken)
    {
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

        var stamp = Stopwatch.GetTimestamp();

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

        Log.SentMail(_logger, receiver, subject, Stopwatch.GetElapsedTime(stamp).TotalMilliseconds);
    }
}
