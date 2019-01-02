using System.Collections.Generic;
using Astruk.Common.Interfaces;

namespace Astruk.Common.Models
{
	public class Map
	{
		public Map(IList<Vertex> vertices, IEnumerable<Region> regions)
		{
			Vertices = vertices;
			Regions = regions;
		}

		public IList<Vertex> Vertices { get; }
		public IEnumerable<Region> Regions { get; }
	}
}