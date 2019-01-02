using System.Collections.Generic;

namespace Astruk.Common.Models
{
	public class Region
	{
		public Region(KeyMapObject keyObject, IList<Vertex> vertices, IEnumerable<MapObject> objects)
		{
			KeyObject = keyObject;
			Vertices = vertices;
			Objects = objects;
		}

		public KeyMapObject KeyObject { get; }
		public IList<Vertex> Vertices { get; }
		public IEnumerable<MapObject> Objects { get; }
	}
}