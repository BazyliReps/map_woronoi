using System;
using System.Collections.Generic;

namespace Astruk.Common.Exceptions
{
	[Serializable]
	public class ValidationException : Exception
	{
		public Dictionary<string, string> ErrorMessages { get; }

		public ValidationException(Dictionary<string, string> errorMessages)
		{
			ErrorMessages = errorMessages;
		}
	}
}