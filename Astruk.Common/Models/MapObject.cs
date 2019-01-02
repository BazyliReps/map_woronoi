using System.Collections.Generic;

namespace Astruk.Common.Models
{
	public class MapObject
	{
		public MapObject(int id, MapObjectType type, IDictionary<string, string> parameters)
		{
			Id = id;
			Type = type;
			Parameters = parameters;
		}

		public int Id { get; }
		public MapObjectType Type { get; }
		private IDictionary<string,string> Parameters { get; }
	}
}