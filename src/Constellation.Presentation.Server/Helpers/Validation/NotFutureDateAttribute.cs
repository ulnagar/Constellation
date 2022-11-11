namespace Constellation.Presentation.Server.Helpers.Validation;

using System.ComponentModel.DataAnnotations;

public class NotFutureDateAttribute : ValidationAttribute
{
	public string GetErrorMessage() =>
		$"Date must not be in the future.";

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
		var date = (DateTime)value;

		if (date > DateTime.Today)
		{
			return new ValidationResult(GetErrorMessage());
		}

		return ValidationResult.Success;
    }
}
