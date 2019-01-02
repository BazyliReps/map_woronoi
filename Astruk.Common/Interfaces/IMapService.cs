using System.Collections.Generic;
using Astruk.Common.Models;

namespace Astruk.Common.Interfaces
{
	public interface IMapService
	{
		Map GenerateMap(IList<Vertex> vertices, IEnumerable<KeyMapObject> keyObjects,
			IEnumerable<MapObjectType> types, IEnumerable<MapObject> objects);
	}
}