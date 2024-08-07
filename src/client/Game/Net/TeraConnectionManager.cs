// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Arise.Client.Game.Net;

internal sealed unsafe partial class TeraConnectionManager : IHostedService
{
    private enum State
    {
        Connected,
        Dropped,
        Disconnected,
    }

    public event Action? Disconnected;

    public event ReadOnlySpanAction<byte, GamePacketCode>? PacketSent;

    private const int HeaderSize = sizeof(ushort) * 2;

    private readonly Queue<FunctionHook> _hooks = new();

    private readonly ConcurrentQueue<byte[]> _receivedPackets = new();

    private readonly CodeManager _codeManager;

    private int _state = (int)State.Connected;

    public TeraConnectionManager(CodeManager codeManager)
    {
        _codeManager = codeManager;
    }

    Task IHostedService.StartAsync(CancellationToken cancellationToken)
    {
        [SuppressMessage("", "CA2000")]
        void AddHook(void* target, void* hook)
        {
            var fh = FunctionHook.Create(_codeManager, target, hook, this);

            fh.IsActive = true;

            _hooks.Enqueue(fh);
        }

        AddHook(S1Connection.Connect, (delegate* unmanaged<S1Connection*, uint, ushort, BOOL>)&ConnectHook);
        AddHook(S1Connection.Disconnect, (delegate* unmanaged<S1Connection*, void>)&DisconnectHook);
        AddHook(S1CommandQueue.RunCommands, (delegate* unmanaged<S1CommandQueue*, void>)&RunCommandsHook);
        AddHook(
            S1ConnectionManager.SendPacket,
            (delegate* unmanaged<S1ConnectionManager*, FName*, byte*, uint, void>)&SendPacketHook);

        return Task.CompletedTask;
    }

    Task IHostedService.StopAsync(CancellationToken cancellationToken)
    {
        // TODO: Audit this; it might cause crashes. If so, we can suppress finalization for the hooks so they stick
        // around until the client fully exits.
        while (_hooks.TryDequeue(out var hook))
            hook.Dispose();

        return Task.CompletedTask;
    }

    private static (FunctionHook Hook, TeraConnectionManager Manager) GetContext()
    {
        var hook = FunctionHook.Current;

        return (hook, Unsafe.As<TeraConnectionManager>(hook.State));
    }

    [UnmanagedCallersOnly]
    private static BOOL ConnectHook(S1Connection* @this, uint ip, ushort port)
    {
        S1GameServerConnection.OnConnect((S1GameServerConnection*)@this, true);

        return true;
    }

    [UnmanagedCallersOnly]
    private static void DisconnectHook(S1Connection* @this)
    {
        var manager = GetContext().Manager;

        // We might have already disconnected due to a network problem.
        _ = Interlocked.CompareExchange(ref manager._state, (int)State.Disconnected, (int)State.Connected);

        manager.Disconnected?.Invoke();
    }

    [UnmanagedCallersOnly]
    private static void RunCommandsHook(S1CommandQueue* @this)
    {
        var (hook, manager) = GetContext();

        if (Interlocked.CompareExchange(
            ref manager._state, (int)State.Disconnected, (int)State.Dropped) == (int)State.Dropped)
            S1GameServerConnection.OnDisconnect((S1GameServerConnection*)@this);

        while (manager._receivedPackets.TryDequeue(out var array))
        {
            fixed (byte* packet = array)
                S1CommandQueue.EnqueuePacket(@this, packet);

            ArrayPool<byte>.Shared.Return(array);
        }

        ((delegate* unmanaged<S1CommandQueue*, void>)hook.OriginalCode)(@this);
    }

    [UnmanagedCallersOnly]
    private static void SendPacketHook(S1ConnectionManager* @this, FName* name, byte* packet, uint length)
    {
        var span = new ReadOnlySpan<byte>(packet, (int)length);

        GetContext().Manager.PacketSent?.Invoke(
            span[HeaderSize..], (GamePacketCode)BinaryPrimitives.ReadUInt16LittleEndian(span[sizeof(ushort)..]));
    }

    public void Disconnect()
    {
        _ = Interlocked.CompareExchange(ref _state, (int)State.Dropped, (int)State.Connected);
    }

    public void EnqueuePacket(GamePacketCode code, scoped ReadOnlySpan<byte> payload)
    {
        var length = HeaderSize + payload.Length;
        var array = ArrayPool<byte>.Shared.Rent(length);
        var span = array.AsSpan(0, length);

        BinaryPrimitives.WriteUInt16LittleEndian(span, (ushort)length);
        BinaryPrimitives.WriteUInt16LittleEndian(span[sizeof(ushort)..], (ushort)code);

        payload.CopyTo(span[HeaderSize..]);

        _receivedPackets.Enqueue(array);
    }
}
