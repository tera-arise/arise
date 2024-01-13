using Arise.Net.Packets;
using static DotNext.Metaprogramming.CodeGenerator;

namespace Arise.Net.Sessions;

public abstract class GameSessionDispatcher<TSession, THandler>
    where TSession : GameSession
    where THandler : class
{
    private static readonly MethodInfo _as = typeof(Unsafe).GetMethod("As", 1, [typeof(object)])!;

    private readonly FrozenDictionary<Type, Action<TSession, GamePacket>> _handlers;

    protected GameSessionDispatcher()
    {
        _handlers = typeof(THandler)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(static method => method.Name == "Handle")
            .Select(static method => (Method: method, Type: method.GetParameters()[1].ParameterType))
            .ToFrozenDictionary(
                static tup => tup.Type,
                static tup =>
                {
                    return Lambda<Action<TSession, GamePacket>>(ctx =>
                    {
                        var (session, packet) = ctx;

                        CallStatic(tup.Method, session, Expression.Call(_as.MakeGenericMethod([tup.Type]), packet));
                    }).Compile();
                });
    }

    public void Dispatch(TSession session, GamePacket packet)
    {
        if (_handlers.TryGetValue(packet.GetType(), out var handler))
            handler(session, packet);
        else
            UnhandledPacket(session, packet);
    }

    protected virtual void UnhandledPacket(TSession session, GamePacket packet)
    {
    }
}
