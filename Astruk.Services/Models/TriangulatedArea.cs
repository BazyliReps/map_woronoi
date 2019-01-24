using Astruk.Common.Models;
using System.Collections.Generic;

namespace Astruk.Services.Models
{
    public class TriangulatedArea
    {
        public List<Triangle> triangles;
        public List<DeluanVertex> points;
        public TriangulatedArea(List<Triangle> triangles, List<DeluanVertex> points)
        {
            this.triangles = triangles;
            this.points = points;

        }

        public IList<Vertex> Vertices { get; set; }
    }
}
