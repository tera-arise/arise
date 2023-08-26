using Arise.Client.Launcher.Logging;

namespace Arise.Client.Launcher;

[SuppressMessage("", "CA1812")]
internal sealed class LauncherApplicationHost : BackgroundService
{
    private readonly IServiceProvider _services;

    private readonly IHostApplicationLifetime _hostLifetime;

    private readonly ReadOnlyMemory<string> _args;

    public LauncherApplicationHost(
        IServiceProvider services, IHostApplicationLifetime hostLifetime, ReadOnlyMemory<string> args)
    {
        _services = services;
        _hostLifetime = hostLifetime;
        _args = args;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // StartWithClassicDesktopLifetime will block until the Avalonia application exits, so avoid blocking the host.
        await Task.Yield();

        Logger.Sink = _services.GetRequiredService<AvaloniaLogSink>();

        try
        {
            _ = AppBuilder
                .Configure(() => ActivatorUtilities.CreateInstance<LauncherApplication>(_services))
                .UseWin32()
                .UseSkia()
                .UseReactiveUI()
                .WithInterFont()
                .StartWithClassicDesktopLifetime(_args.ToArray(), ShutdownMode.OnMainWindowClose);
        }
        finally
        {
            Logger.Sink = null;
        }

        _hostLifetime.StopApplication();
    }
}
