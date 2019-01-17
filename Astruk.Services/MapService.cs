using System;
using System.Collections.Generic;
using System.Linq;
using Astruk.Common.Interfaces;
using Astruk.Common.Models;
using Astruk.Services.Helpers;

namespace Astruk.Services
{
	internal class MapService : IMapService
	{
		public TestMap GenerateMap(IList<Vertex> Vertices, IEnumerable<KeyMapObject> KeyObjects, IEnumerable<MapObjectType> Types, IEnumerable<MapObject> Objects)
		{
            Triangulator TriangulationMaker = new Triangulator();
            List<Point> Points = new List<Point>();

            foreach (var keyObject in KeyObjects) {
                Points.Add(new Point(keyObject.X, keyObject.Y));
            }

            TestMap data = TriangulationMaker.Triangulate(Points, Vertices);


            return data;

		}


        


    }
}
