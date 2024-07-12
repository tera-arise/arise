// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Arise.Server.Gateway.Cryptography;

public sealed class Argon2idPasswordStrategy : PasswordStrategy
{
    public static Argon2idPasswordStrategy Instance { get; } = new();

    // https://cheatsheetseries.owasp.org/cheatsheets/Password_Storage_Cheat_Sheet.html#argon2id
    private static readonly Argon2id _algorithm = new(new()
    {
        DegreeOfParallelism = 1,
        MemorySize = 46 * 1024,
        NumberOfPasses = 1,
    });

    private Argon2idPasswordStrategy()
    {
    }

    public override byte[] GenerateSalt()
    {
        // https://www.rfc-editor.org/rfc/rfc9106.html#section-4
        //
        // At the time of writing, the salt length for Argon2id is always 16 in libsodium. While unlikely, NSec's
        // MinSaltSize and MaxSaltSize properties could change in the future if libsodium's requirements change. So, we
        // just play it safe and use a constant here to avoid nasty surprises.
        return RandomNumberGenerator.GetBytes(16);
    }

    public override byte[] CalculateHash(scoped ReadOnlySpan<char> password, scoped ReadOnlySpan<byte> salt)
    {
        // https://www.rfc-editor.org/rfc/rfc9106.html#section-4
        return _algorithm.DeriveBytes(MemoryMarshal.AsBytes(password), salt, count: 32);
    }
}
