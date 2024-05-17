// SPDX-License-Identifier: AGPL-3.0-or-later

using Arise.DataAnnotations;

namespace Arise.Gateway;

public sealed class AccountsCreateRequest
{
    [Email(allowTopLevelDomains: true, allowInternational: true)]
    public required string Email { get; init; }

    [Password]
    public required string Password { get; init; }
}
