// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Arise.Server.Gateway.Cryptography;

internal static class PasswordStrategyProvider
{
    public static PasswordStrategy GetLatestStrategy()
    {
        return GetStrategy(AccountPasswordKind.Argon2id);
    }

    public static PasswordStrategy GetStrategy(AccountPasswordKind kind)
    {
        return kind switch
        {
            AccountPasswordKind.Pbkdf2 => Pbkdf2PasswordStrategy.Instance,
            AccountPasswordKind.Argon2id => Argon2idPasswordStrategy.Instance,
            _ => throw new UnreachableException(),
        };
    }

    public static AccountPasswordKind GetKind(PasswordStrategy strategy)
    {
        return strategy switch
        {
            Pbkdf2PasswordStrategy => AccountPasswordKind.Pbkdf2,
            Argon2idPasswordStrategy => AccountPasswordKind.Argon2id,
            _ => throw new UnreachableException(),
        };
    }
}
