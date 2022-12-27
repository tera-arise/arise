namespace Arise.Server.Web.Cryptography;

public sealed class Pbkdf2PasswordStrategy : PasswordStrategy
{
    private const int Length = SHA512.HashSizeInBytes;

    public static Pbkdf2PasswordStrategy Instance { get; } = new();

    public override AccountPasswordKind Kind => AccountPasswordKind.Pbkdf2;

    private Pbkdf2PasswordStrategy()
    {
    }

    public override byte[] GenerateSalt()
    {
        return RandomNumberGenerator.GetBytes(Length);
    }

    public override byte[] CalculateHash(ReadOnlySpan<char> password, ReadOnlySpan<byte> salt)
    {
        // https://cheatsheetseries.owasp.org/cheatsheets/Password_Storage_Cheat_Sheet.html#pbkdf2
        return Rfc2898DeriveBytes.Pbkdf2(password, salt, 120000, HashAlgorithmName.SHA512, Length);
    }
}
