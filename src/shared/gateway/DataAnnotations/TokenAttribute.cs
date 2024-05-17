// SPDX-License-Identifier: AGPL-3.0-or-later

using Arise.Cryptography;

namespace Arise.DataAnnotations;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed class TokenAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        return value == null || TokenGenerator.IsTokenValid(Unsafe.As<string>(value))
            ? ValidationResult.Success
            : new ValidationResult(
                "Token format is invalid.", validationContext?.MemberName is { } name ? [name] : null);
    }
}
