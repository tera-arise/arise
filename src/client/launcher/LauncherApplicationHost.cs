using Arise.Client.Launcher.Logging;

namespace Arise.Client.Launcher;

[SuppressMessage("", "CA1812")]
internal sealed class LauncherApplicationHost : BackgroundService
{
    private readonly IServiceProvider _services;

    private readonly IHostApplicationLifetime _lifetime;

    private readonly ReadOnlyMemory<string> _args;

    public LauncherApplicationHost(
        IServiceProvider services, IHostApplicationLifetime lifetime, ReadOnlyMemory<string> args)
    {
        _services = services;
        _lifetime = lifetime;
        _args = args;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();

        Logger.Sink = _services.GetRequiredService<AvaloniaLogSink>();

        _ = AppBuilder
            .Configure(() => new LauncherApplication(_services))
            .UseWin32()
            .UseSkia()
            .UseReactiveUI()
            .WithInterFont()
            .StartWithClassicDesktopLifetime(_args.ToArray(), ShutdownMode.OnMainWindowClose);

        _lifetime.StopApplication();
    }
}
