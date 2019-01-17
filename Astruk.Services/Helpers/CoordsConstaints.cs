using Astruk.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astruk.Services.Helpers
{
    class CoordsConstaints
    {


        public CoordsConstaints(IList<Vertex> Vertices)
        {
            this.XMin = double.MaxValue;
            this.XMax = -double.MaxValue;
            this.YMin = double.MaxValue;
            this.YMax = -double.MaxValue;

            foreach (var vertex in Vertices) {
                if (vertex.X > XMax) {
                    XMax = vertex.X;
                } else if (vertex.X < XMin) {
                    XMin = vertex.X;
                }

                if (vertex.Y > YMax) {
                    YMax = vertex.Y;
                } else if (vertex.Y < YMin) {
                    YMin = vertex.Y;
                }
            }
        }

        public double XMin { get; }
        public double XMax { get; }
        public double YMin { get; }
        public double YMax { get; }
    }
}
