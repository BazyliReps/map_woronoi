using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astruk.Common.Models
{
    public class TestMap
    {
        public List<Triangle> triangles;
        public List<DeluanVertex> points;
        public TestMap(List<Triangle> triangles, List<DeluanVertex> points)
        {
            this.triangles = triangles;
            this.points = points;
        }
        

    }
}
