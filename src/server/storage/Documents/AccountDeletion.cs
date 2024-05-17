// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Arise.Server.Storage.Documents;

public sealed class AccountDeletion
{
    public required DateTime Due { get; init; }

    public required AccountToken? Verification { get; set; }
}
