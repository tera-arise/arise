// SPDX-License-Identifier: AGPL-3.0-or-later
namespace Arise.Client.Launcher.Controls;

public class UriValidation : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        if (value is not string str)
            return false;

        try
        {
            _ = new Uri(str);
            return true;
        }
        catch (UriFormatException)
        {
            return false;
        }
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not string str)
            return new ValidationResult("Address is not a string.");

        try
        {
            _ = new Uri(str);
            return ValidationResult.Success;
        }
        catch (UriFormatException e)
        {
            return new ValidationResult(e.Message);
        }
    }
}
