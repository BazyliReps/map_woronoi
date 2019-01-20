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
				new Vertex(14.625, 24),
				new Vertex(41.25, 14.875),
				new Vertex(72.875, 14.75),
				new Vertex(85.875, 24.625),
				new Vertex(70.5, 24.5),
				new Vertex(61.875, 28.875),
				new Vertex(74.375, 29.25),
				new Vertex(88.125, 33.375),
				new Vertex(92.5, 47.25),
				new Vertex(89.875, 58.125),
				new Vertex(80.375, 68.5),
				new Vertex(56, 75.375),
				new Vertex(37.75, 72.625),
				new Vertex(18.875, 68),
				new Vertex(5.625, 52.875),
				new Vertex(5.75, 35.25)
			}, new List<Region>
			{
				new Region(new KeyMapObject(1, 20, 20, "Dupa wołowa"), new List<Vertex>
				{
					new Vertex(50, 45),
					new Vertex(14.625, 24),
					new Vertex(41.25, 14.875),
					new Vertex(72.875, 14.75),
					new Vertex(85.875, 24.625),
					new Vertex(70.5, 24.5),
					new Vertex(61.875, 28.875)
				}, new List<MapObject>()),
				new Region(new KeyMapObject(2, 70, 35, "Świński ogon"), new List<Vertex>
				{
					new Vertex(50, 45),
					new Vertex(61.875, 28.875),
					new Vertex(74.375, 29.25),
					new Vertex(88.125, 33.375),
					new Vertex(92.5, 47.25),
					new Vertex(89.875, 58.125),
					new Vertex(80.375, 68.5),
					new Vertex(56, 75.375)
				}, new List<MapObject>()),
				new Region(new KeyMapObject(3, 25, 60, "Pasztet króliczy"), new List<Vertex>
				{
					new Vertex(50, 45),
					new Vertex(56, 75.375),
					new Vertex(37.75, 72.625),
					new Vertex(18.875, 68),
					new Vertex(5.625, 52.875),
					new Vertex(5.75, 35.25),
					new Vertex(14.625, 24)
				}, new List<MapObject>())
			});
		}
	}
}