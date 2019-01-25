using System;
using System.Collections.Generic;
using System.Linq;
using Astruk.Common.Interfaces;
using Astruk.Common.Models;

namespace Astruk.Services.Validation
{
	internal class TypesExist : IValidationRuleWithParameters<IEnumerable<MapObject>>
	{
		public TypesExist()
		{
			ParameterList = new List<object>();
			ParameterTypes = new List<Type> {typeof(IEnumerable<MapObjectType>)};
		}

		private string NonExistingTypeName { get; set; }

		public string ErrorMessage => $"Type {NonExistingTypeName} was not declared";
		public string ErrorKey => "mapObjects";
		public bool IsCritical => false;

		public bool IsValid(IEnumerable<MapObject> obj)
		{
			var types = (IEnumerable<MapObjectType>) ParameterList[0];
			foreach (var mapObject in obj)
			{
				if (types.Any(x => x.GetHashCode() == mapObject.Type.GetHashCode())) continue;
				NonExistingTypeName = mapObject.Type.Name;
				return false;
			}

			return true;
		}

		public List<object> ParameterList { get; }
		public List<Type> ParameterTypes { get; }
	}
}