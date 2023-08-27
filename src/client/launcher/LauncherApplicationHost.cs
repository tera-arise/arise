using Arise.Client.Launcher.Logging;

namespace Arise.Client.Launcher;

[RegisterSingleton<LauncherApplicationHost>]
internal sealed class LauncherApplicationHost : IHostedService
{
    private readonly IServiceProvider _services;

    private readonly IHostApplicationLifetime _hostLifetime;

    private readonly AvaloniaLogSink _logSink;

    public LauncherApplicationHost(
        IServiceProvider services, IHostApplicationLifetime hostLifetime, AvaloniaLogSink logSink)
    {
        _services = services;
        _hostLifetime = hostLifetime;
        _logSink = logSink;
    }

    Task IHostedService.StartAsync(CancellationToken cancellationToken)
    {
        var thread = new Thread(() =>
        {
            Logger.Sink = _logSink;

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
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();

        return Task.CompletedTask;
    }

    Task IHostedService.StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
