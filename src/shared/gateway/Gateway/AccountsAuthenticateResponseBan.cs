// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Arise.Gateway;

public sealed class AccountsAuthenticateResponseBan
{
    public required DateTime Expiry { get; init; }

    public required string? Reason { get; init; }
}
