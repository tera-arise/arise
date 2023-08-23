using Arise.Server.Web.Cryptography;

namespace Arise.Server.Web.DataAnnotations;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed class TokenAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        return value == null || TokenGenerator.IsTokenValid((string)value)
            ? ValidationResult.Success
            : new ValidationResult(
                "Token format is invalid.", validationContext?.MemberName is string name ? [name] : null);
    }
}
