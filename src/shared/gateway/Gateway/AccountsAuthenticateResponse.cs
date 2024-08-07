// SPDX-License-Identifier: AGPL-3.0-or-later

using Arise.DataAnnotations;

namespace Arise.Gateway;

public sealed class AccountsAuthenticateResponse
{
    public required bool IsVerifying { get; init; }

    public required bool IsChangingEmail { get; init; }

    public required bool IsRecovered { get; init; }

    public required DateTime? DeletionDue { get; init; }

    public required AccountsAuthenticateResponseBan? Ban { get; init; }

    [Token]
    public required string? SessionTicket { get; init; }
}
