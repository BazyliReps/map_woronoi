using System.Collections.Generic;
using System.Linq;
using Astruk.Common.Interfaces;
using Astruk.Common.Models;

namespace Astruk.Services.Validation
{
	internal class MustBeTriangulable : IValidationRule<IEnumerable<KeyMapObject>>
	{
		public string ErrorMessage => "Musza być więcej niż 3 punkty kluczowe";
		public string ErrorKey => "keyObjects";
		public bool IsCritical => false;

		public bool IsValid(IEnumerable<KeyMapObject> obj)
		{
			return obj.Count() >= 3;
		}
	}
}