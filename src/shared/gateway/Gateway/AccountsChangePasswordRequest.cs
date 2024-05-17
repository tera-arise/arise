// SPDX-License-Identifier: AGPL-3.0-or-later

using Arise.DataAnnotations;

namespace Arise.Gateway;

public sealed class AccountsChangePasswordRequest
{
    [Password]
    public required string Password { get; init; }
}
