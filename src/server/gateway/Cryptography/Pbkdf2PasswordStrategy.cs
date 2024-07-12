// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Arise.Server.Gateway.Cryptography;

public sealed class Pbkdf2PasswordStrategy : PasswordStrategy
{
    public static Pbkdf2PasswordStrategy Instance { get; } = new();

    private Pbkdf2PasswordStrategy()
    {
    }

    public override byte[] GenerateSalt()
    {
        // https://nvlpubs.nist.gov/nistpubs/Legacy/SP/nistspecialpublication800-132.pdf
        return RandomNumberGenerator.GetBytes(16);
    }

    public override byte[] CalculateHash(scoped ReadOnlySpan<char> password, scoped ReadOnlySpan<byte> salt)
    {
        // https://cheatsheetseries.owasp.org/cheatsheets/Password_Storage_Cheat_Sheet.html#pbkdf2
        return Rfc2898DeriveBytes.Pbkdf2(
            password, salt, iterations: 210_000, HashAlgorithmName.SHA512, outputLength: SHA512.HashSizeInBytes);
    }
}
