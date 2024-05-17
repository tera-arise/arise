// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Arise.Server.Gateway.Email;

[RegisterSingleton<EmailSender>]
[SuppressMessage("", "CA1001")]
internal sealed partial class EmailSender : IHostedService
{
    private static partial class Log
    {
        [LoggerMessage(0, LogLevel.Information, "Sent email {Subject} to {Receiver} in {ElapsedMs:0.0000} ms")]
        public static partial void SentEmail(
            ILogger<EmailSender> logger, string receiver, string subject, double elapsedMs);

        [LoggerMessage(1, LogLevel.Information, "Email {Subject} to {Receiver} was dropped")]
        public static partial void EmailDropped(
            ILogger<EmailSender> logger, Exception? exception, string receiver, string subject);
    }

    private readonly CancellationTokenSource _cts = new();

    private readonly TaskCompletionSource _sendDone = new(TaskCreationOptions.RunContinuationsAsynchronously);

    private readonly Channel<(string Receiver, string Subject, string Content)> _sendQueue =
        Channel.CreateUnbounded<(string Receiver, string Subject, string Content)>(new()
        {
            SingleReader = true,
        });

    private readonly IServiceScopeFactory _scopeFactory;

    private readonly IOptions<GatewayOptions> _options;

    private readonly ILogger<EmailSender> _logger;

    public EmailSender(IServiceScopeFactory scopeFactory, IOptions<GatewayOptions> options, ILogger<EmailSender> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options;
        _logger = logger;
    }

    public void EnqueueEmail(string receiver, string subject, string content)
    {
        _ = _sendQueue.Writer.TryWrite((receiver, subject, content));
    }

    Task IHostedService.StartAsync(CancellationToken cancellationToken)
    {
        var ct = _cts.Token;

        _ = Task.Run(() => SendEmailsAsync(ct), ct);

        return Task.CompletedTask;
    }

    async Task IHostedService.StopAsync(CancellationToken cancellationToken)
    {
        _sendQueue.Writer.Complete();

        // Signal the send task to shut down.
        await _cts.CancelAsync();

        // Note that the send task is not expected to encounter any exceptions.
        await _sendDone.Task;

        // The task is done; safe to dispose this now.
        _cts.Dispose();
    }

    private async Task SendEmailsAsync(CancellationToken cancellationToken)
    {
        try
        {
            await foreach (var (receiver, subject, content) in _sendQueue.Reader.ReadAllAsync(cancellationToken))
            {
                var stopwatch = SlimStopwatch.Create();

                using var scope = _scopeFactory.CreateAsyncScope();

                var client = scope.ServiceProvider.GetRequiredService<ISendGridClient>();
                var message = new SendGridMessage
                {
                    From = new(_options.Value.EmailAddress, ThisAssembly.GameTitle),
                    Subject = $"{subject} | {ThisAssembly.GameTitle}",
                    PlainTextContent = $"""
                    Hi!

                    {content}

                    Regards,
                    {ThisAssembly.GameTitle} Team
                    """.ReplaceLineEndings("\r\n"), // Emails use CRLF.
                };

                message.AddTo(receiver);

                try
                {
                    _ = await client.SendEmailAsync(message, cancellationToken);
                }
                catch (Exception e) when (
                    e is HttpRequestException or TimeoutException or RequestErrorException or SendGridInternalException)
                {
                    Log.EmailDropped(_logger, e, receiver, subject);

                    continue;
                }

                Log.SentEmail(_logger, receiver, subject, stopwatch.Elapsed.TotalMilliseconds);
            }
        }
        catch (OperationCanceledException)
        {
            // StopAsync() was called.
        }
        finally
        {
            _sendDone.SetResult();
        }
    }
}
