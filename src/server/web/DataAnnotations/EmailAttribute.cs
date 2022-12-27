namespace Arise.Server.Web.DataAnnotations;

// TODO: https://github.com/jstedfast/EmailValidation/issues/41
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed class EmailAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        return value == null || EmailValidator.Validate((string)value, true, true)
            ? ValidationResult.Success
            : new ValidationResult(
                "Email address format is invalid.",
                validationContext?.MemberName is string name ? new[] { name } : null);
    }
}
