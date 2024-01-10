using Arise.Client.Game.Memory;

namespace Arise.Client.Game.Launcher;

internal sealed unsafe partial class TeraLauncherProxyManager : IHostedService
{
    private readonly Queue<FunctionHook> _hooks = new();

    private readonly IOptions<GameOptions> _options;

    private readonly CodeManager _codeManager;

    public TeraLauncherProxyManager(IOptions<GameOptions> options, CodeManager codeManager)
    {
        _options = options;
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

        AddHook(
            S1LauncherProxy.SendAccountNameRequest,
            (delegate* unmanaged<S1LauncherProxy*, FString*>)&SendAccountNameRequestHook);
        AddHook(
            S1LauncherProxy.SendSessionTicketRequest,
            (delegate* unmanaged<S1LauncherProxy*, TArray<byte>*>)&SendSessionTicketRequestHook);
        AddHook(
            S1LauncherProxy.SendServerListRequest,
            (delegate* unmanaged<S1LauncherProxy*, BOOL, int, void>)&SendServerListRequestHook);

        return Task.CompletedTask;
    }

    Task IHostedService.StopAsync(CancellationToken cancellationToken)
    {
        while (_hooks.TryDequeue(out var hook))
            hook.Dispose();

        return Task.CompletedTask;
    }

    private static TeraLauncherProxyManager GetManager()
    {
        return Unsafe.As<TeraLauncherProxyManager>(FunctionHook.Current.State);
    }

    [UnmanagedCallersOnly]
    private static FString* SendAccountNameRequestHook(S1LauncherProxy* @this)
    {
        fixed (char* ptr = GetManager()._options.Value.AccountName)
            return FString.__ctor(&@this->account_name, ptr);
    }

    [UnmanagedCallersOnly]
    private static TArray<byte>* SendSessionTicketRequestHook(S1LauncherProxy* @this)
    {
        var ticket = Encoding.UTF8.GetBytes(GetManager()._options.Value.SessionTicket);
        var array = &@this->session_ticket;

        array->elements = TeraMemory.AllocArray<byte>(ticket.Length);
        array->count = array->capacity = ticket.Length;

        ticket.CopyTo(new Span<byte>(array->elements, ticket.Length));

        return array;
    }

    [UnmanagedCallersOnly]
    private static void SendServerListRequestHook(S1LauncherProxy* @this, BOOL unsorted, int sorting)
    {
        var list = &@this->server_list;

        list->last_server_id = 42;
        list->sort_criterion = S1ServerListSortCriteria.S1_SERVER_LIST_SORTING_NONE;

        var server = list->servers.elements = TeraMemory.AllocArray<S1ServerInfo>(1);

        server->id = list->last_server_id;
        server->available = true;
        server->unavailable_message = default;

        // No need to initialize the host, address, or port since we manage the connection ourselves.
        server->host = default;
        server->address = default;
        server->port = 0;

        static void InitializeString(FString* ptr, string value)
        {
            fixed (char* pinned = value)
                _ = FString.__ctor(ptr, pinned);
        }

        InitializeString(&server->name, ThisAssembly.GameTitle);
        InitializeString(&server->category, "<font color=\"#ff7f00\">PvEvP</font>");
        InitializeString(&server->title, $"<font color=\"#ffffff\">{ThisAssembly.GameTitle}</font>");
        InitializeString(&server->queue, "<font color=\"#7f7f7f\">N/A</font>");
        InitializeString(&server->population, "<font color=\"#7fff7f\">High</font>");

        // Notify S1LobbySceneServer that the server list is ready.
        S1.ServerDataEvent.VFT->ILauncherEvent.OnServerList(
            (ILauncherEvent*)Unsafe.AsPointer(ref S1.ServerDataEvent), true, list);
    }
}
