namespace Arise.Cryptography;

public static class TokenGenerator
{
    [SuppressMessage("", "CA1308")]
    public static string GenerateToken()
    {
        // https://cheatsheetseries.owasp.org/cheatsheets/Input_Validation_Cheat_Sheet.html#semantic-validation
        return Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();
    }

    public static bool IsTokenValid(string token)
    {
        return token.Length == 64 && token.All(char.IsAsciiHexDigitLower);
    }
}
