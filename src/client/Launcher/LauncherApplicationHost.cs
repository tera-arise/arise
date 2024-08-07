// SPDX-License-Identifier: AGPL-3.0-or-later

using Arise.Client.Logging;

namespace Arise.Client.Launcher;

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
        var thread = new Thread(static state =>
        {
            var @this = Unsafe.As<LauncherApplicationHost>(state!);

            Logger.Sink = @this._logSink;

            try
            {
                _ = Program
                    .BuildAvaloniaApp(() => ActivatorUtilities.CreateInstance<LauncherApplication>(@this._services))
                    .StartWithClassicDesktopLifetime([], ShutdownMode.OnMainWindowClose);
            }
            finally
            {
                Logger.Sink = null;
            }

            @this._hostLifetime.StopApplication();
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start(this);

        return Task.CompletedTask;
    }

    Task IHostedService.StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
