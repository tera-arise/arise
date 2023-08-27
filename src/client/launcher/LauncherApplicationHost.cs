using Arise.Client.Launcher.Logging;

namespace Arise.Client.Launcher;

[RegisterSingleton]
[SuppressMessage("", "CA1812")]
internal sealed class LauncherApplicationHost : IHostedService
{
    private readonly IServiceProvider _services;

    private readonly IHostApplicationLifetime _hostLifetime;

    public LauncherApplicationHost(
        IServiceProvider services, IHostApplicationLifetime hostLifetime)
    {
        _services = services;
        _hostLifetime = hostLifetime;
    }

    Task IHostedService.StartAsync(CancellationToken cancellationToken)
    {
        _ = Task.Run(
            () =>
            {
                Logger.Sink = _services.GetRequiredService<AvaloniaLogSink>();

                try
                {
                    _ = AppBuilder
                        .Configure(() => ActivatorUtilities.CreateInstance<LauncherApplication>(_services))
                        .UseWin32()
                        .UseSkia()
                        .UseReactiveUI()
                        .WithInterFont()
                        .StartWithClassicDesktopLifetime([], ShutdownMode.OnMainWindowClose);
                }
                finally
                {
                    Logger.Sink = null;
                }

                _hostLifetime.StopApplication();
            },
            cancellationToken);

        return Task.CompletedTask;
    }

    Task IHostedService.StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
