// SPDX-License-Identifier: AGPL-3.0-or-later

using Arise.Client.Game.Net;

namespace Arise.Client.Game;

internal sealed class GameApplicationHost : IHostedService
{
    private readonly BridgeProtectionComponent _protectionComponent;

    private readonly Action _wake;

    public GameApplicationHost(GameClient client, Action wake)
    {
        _protectionComponent = client.Session.Module.Protection;
        _wake = wake;
    }

    Task IHostedService.StartAsync(CancellationToken cancellationToken)
    {
        // Start watching for code changes, debuggers, etc.
        _protectionComponent.Start();

        // Wake up the game now that we are fully initialized.
        _wake();

        return Task.CompletedTask;
    }

    Task IHostedService.StopAsync(CancellationToken cancellationToken)
    {
        // Stop the protection tasks so we can undo our code changes.
        _protectionComponent.Stop();

        return Task.CompletedTask;
    }
}
