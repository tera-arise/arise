// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Arise.Gateway;

public sealed class AccountsVerifyEmailChangeResponse
{
    [Email(allowTopLevelDomains: true, allowInternational: true)]
    public required string Email { get; init; }
}
