// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Arise.Gateway;

public sealed class GatewayHttpClientLauncherController : GatewayHttpClientController
{
    internal GatewayHttpClientLauncherController(GatewayHttpClient client)
        : base(client, "Launcher")
    {
    }

    public ValueTask<LauncherHelloResponse> HelloAsync()
    {
        return SendAsync(HttpMethod.Get, "Hello", Context.LauncherHelloResponse, credentials: null);
    }
}
