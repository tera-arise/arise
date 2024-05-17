// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Arise.Server.Storage.Documents;

public sealed class AccountBan
{
    public required DateTime Expiry { get; set; }

    public required string Reason { get; set; }
}
