// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Arise.Server.Storage.Documents;

public sealed class AccountEmail
{
    public required string OriginalAddress { get; init; }

    public required string NormalizedAddress { get; init; }

    public required AccountToken? Verification { get; set; }
}
