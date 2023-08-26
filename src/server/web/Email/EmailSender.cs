namespace Arise.Server.Web.Email;

[RegisterTransient]
internal sealed partial class EmailSender
{
    private static partial class Log
    {
        [LoggerMessage(0, LogLevel.Information, "Sent email {Subject} to {Receiver} in {ElapsedMs:0.0000} ms")]
        public static partial void SentEmail(
            ILogger<EmailSender> logger, string receiver, string subject, double elapsedMs);
    }

    private readonly IHostEnvironment _environment;

    private readonly IOptionsMonitor<WebOptions> _options;

    private readonly ILogger<EmailSender> _logger;

    private readonly ISendGridClient _client;

    public EmailSender(
        IHostEnvironment environment,
        IOptionsMonitor<WebOptions> options,
        ILogger<EmailSender> logger,
        ISendGridClient client)
    {
        _environment = environment;
        _options = options;
        _logger = logger;
        _client = client;
    }

    public async ValueTask SendAsync(
        string receiver, string subject, string content, CancellationToken cancellationToken)
    {
        var stopwatch = SlimStopwatch.Create();

        var suffix = _environment.IsStaging() ? " (Staging)" : string.Empty;
        var message = new SendGridMessage
        {
            From = new(_options.CurrentValue.EmailAddress, $"TERA Arise{suffix}"),
            Subject = $"{subject} | TERA Arise{suffix}",
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
            throw new EmailException("A low-level connection error occurred.", e);
        }
        catch (TaskCanceledException e)
        {
            throw new EmailException("A connection timeout occurred.", e);
        }
        catch (RequestErrorException e)
        {
            throw new EmailException("A SendGrid request error occurred.", e);
        }
        catch (SendGridInternalException e)
        {
            throw new EmailException("An internal SendGrid error occurred.", e);
        }

        Log.SentEmail(_logger, receiver, subject, stopwatch.Elapsed.TotalMilliseconds);
    }
}
