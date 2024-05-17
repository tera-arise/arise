// SPDX-License-Identifier: AGPL-3.0-or-later

using Arise.DataAnnotations;

namespace Arise.Gateway;

public sealed class AccountsVerifyDeletionRequest
{
    [Token]
    public required string Token { get; init; }
}
