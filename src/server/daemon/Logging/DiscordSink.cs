namespace Arise.Server.Daemon.Logging;

internal sealed class DiscordSink : IDisposable, ILogEventSink
{
    private readonly DiscordSocketClient _client;

    private readonly ITextChannel _channel;

    public DiscordSink(string botToken, ulong guildId, ulong channelId)
    {
        _client = new DiscordSocketClient(new()
        {
            GatewayIntents = GatewayIntents.None, // Be a good citizen.
        });

        _client.LoginAsync(TokenType.Bot, botToken).GetAwaiter().GetResult();
        _client.StartAsync().GetAwaiter().GetResult();

        _channel = _client
            .Rest
            .GetGuildAsync(guildId)
            .GetAwaiter()
            .GetResult()
            .GetTextChannelAsync(channelId)
            .GetAwaiter()
            .GetResult();
    }

    public void Dispose()
    {
        _client.Dispose();
    }

    public void Emit(LogEvent logEvent)
    {
        static string WrapInBlock(object? value, int limit)
        {
            var str = value?.ToString()?.Trim() ?? string.Empty;
            var length = 0;

            // Avoid LINQ for performance.
            foreach (var r in str.EnumerateRunes())
                length++;

            return $"""
            ```
            {(length <= limit ? str : str[..limit])}
            ```
            """;
        }

        var builder = new EmbedBuilder
        {
            Title = logEvent.Level.ToString(),
            Description = WrapInBlock(logEvent.RenderMessage(CultureInfo.InvariantCulture), 4096),
            Timestamp = logEvent.Timestamp,
            Color = logEvent.Level switch
            {
                LogEventLevel.Verbose => new(128, 128, 128),
                LogEventLevel.Debug => new(192, 192, 192),
                LogEventLevel.Information => new(255, 255, 255),
                LogEventLevel.Warning => new(255, 255, 0),
                LogEventLevel.Error => new(128, 0, 0),
                LogEventLevel.Fatal => new(255, 255, 255),
                _ => throw new UnreachableException(),
            },
        };

        if (logEvent.Exception is { } ex)
        {
            builder.Title += " (Exception)";

            _ = builder
                .AddField("Type", WrapInBlock(ex.GetType(), 1024))
                .AddField("Message", WrapInBlock(ex.Message, 1024))
                .AddField("Trace", WrapInBlock(ex.StackTrace, 1024));
        }

        _ = _channel.SendMessageAsync(embed: builder.Build()).GetAwaiter().GetResult();
    }
}
