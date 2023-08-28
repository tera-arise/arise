namespace Arise.Cryptography;

public abstract class PasswordStrategy
{
    [SuppressMessage("", "CA1308")]
    public static string GeneratePassword()
    {
        return Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();
    }

    public static bool IsPasswordValid(string password)
    {
        // https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html#implement-proper-password-strength-controls
        return password.Length is >= 8 and <= 128 && password.All(static ch => char.IsAscii(ch) && !char.IsControl(ch));
    }

    public abstract byte[] GenerateSalt();

    public abstract byte[] CalculateHash(scoped ReadOnlySpan<char> password, scoped ReadOnlySpan<byte> salt);
}
