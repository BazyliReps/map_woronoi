using System.Collections.Generic;
using Astruk.Common.Exceptions;
using Astruk.Common.Interfaces;

namespace Astruk.Common.Utility
{
	public class ValidationResult<T>
	{
		public bool IsValid => FailedValidations.Count == 0;

		public List<IValidationRule<T>> FailedValidations { get; }

		public ValidationResult()
		{
			FailedValidations = new List<IValidationRule<T>>();
		}

		public void ThrowDefaultException()
		{
			var errorList = new Dictionary<string, string>();
			foreach (var failedValidation in FailedValidations)
			{
				errorList.Add(failedValidation.ErrorKey, failedValidation.ErrorMessage);
			}

			throw new ValidationException(errorList);
		}
	}
}