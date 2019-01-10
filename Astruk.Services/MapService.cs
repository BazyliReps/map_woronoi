using System.Collections.Generic;
using Astruk.Common.Interfaces;
using Astruk.Common.Models;

namespace Astruk.Services
{
	internal class MapService : IMapService
	{
		public Map GenerateMap(IList<Vertex> vertices, IEnumerable<KeyMapObject> keyObjects,
			IEnumerable<MapObjectType> types, IEnumerable<MapObject> objects)
		{
			// Hardcoded for test purposes
			return new Map(new List<Vertex>
			{
				new Vertex(0, 14.625, 24),
				new Vertex(1, 41.25, 14.875),
				new Vertex(2, 72.875, 14.75),
				new Vertex(3, 85.875, 24.625),
				new Vertex(4, 70.5, 24.5),
				new Vertex(5, 61.875, 28.875),
				new Vertex(6, 74.375, 29.25),
				new Vertex(7, 88.125, 33.375),
				new Vertex(8, 92.5, 47.25),
				new Vertex(9, 89.875, 58.125),
				new Vertex(10, 80.375, 68.5),
				new Vertex(11, 56, 75.375),
				new Vertex(11, 37.75, 72.625),
				new Vertex(11, 18.875, 68),
				new Vertex(11, 5.625, 52.875),
				new Vertex(11, 5.75, 35.25)
			}, new List<Region>());
		}
	}
}
