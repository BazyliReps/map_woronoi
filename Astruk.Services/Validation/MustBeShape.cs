using System.Collections.Generic;
using Astruk.Common.Interfaces;
using Astruk.Common.Models;

namespace Astruk.Services.Validation
{
	internal class MustBeShape : IValidationRule<IList<Vertex>>
	{
		public string ErrorMessage => "Musi być więcej niż 3 wierzchołki";
		public string ErrorKey => "vertices";
		public bool IsCritical => false;

		public bool IsValid(IList<Vertex> obj)
		{
			return obj.Count >= 3;
		}
	}
}