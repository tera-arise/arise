// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Arise.Server.Storage.Documents;

public sealed class AccountRecovery
{
    public required AccountPassword Password { get; init; }

    public required DateTime Expiry { get; init; }
}
