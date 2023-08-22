namespace Arise.Server.Net.Handlers;

[RegisterSingleton]
[SuppressMessage("", "CA1812")]
internal sealed partial class GamePacketHandler
{
    private static partial class Log
    {
        [LoggerMessage(0, LogLevel.Warning, "No game packet handler found for {Channel}:{Code} from {EndPoint}")]
        public static partial void NoHandlerFound(
            ILogger logger, GameConnectionChannel channel, Enum code, IPEndPoint endPoint);
    }

    private static readonly MethodInfo _unsafeAs = typeof(Unsafe).GetMethod("As", 1, new[] { typeof(object) })!;

    private static readonly FrozenDictionary<Type, Action<GameSession, GamePacket>> _handlers =
        typeof(GamePacketHandler)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(static method => method.Name == "Handle")
            .ToFrozenDictionary(
                static method => method.GetParameters()[1].ParameterType,
                static method =>
                {
                    var session = Expression.Parameter(typeof(GameSession));
                    var packet = Expression.Parameter(typeof(GamePacket));

                    return Expression.Lambda<Action<GameSession, GamePacket>>(
                        Expression.Call(
                            method,
                            session,
                            Expression.Call(
                                _unsafeAs.MakeGenericMethod(new[] { method.GetParameters()[1].ParameterType }),
                                packet)),
                        session,
                        packet).Compile();
                });

    private readonly ILogger<GamePacketHandler> _logger;

    public GamePacketHandler(ILogger<GamePacketHandler> logger)
    {
        _logger = logger;
    }

    public void Dispatch(GameSession session, GamePacket packet)
    {
        if (_handlers.TryGetValue(packet.GetType(), out var handler))
            handler(session, packet);
        else
            Log.NoHandlerFound(
                _logger,
                packet.Channel,
                packet.Channel switch
                {
                    GameConnectionChannel.Tera => (TeraGamePacketCode)packet.RawCode,
                    GameConnectionChannel.Arise => (AriseGamePacketCode)packet.RawCode,
                    _ => throw new UnreachableException(),
                },
                session.EndPoint);
    }
}
