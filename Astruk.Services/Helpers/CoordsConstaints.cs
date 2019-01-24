using Astruk.Common.Models;
using System.Collections.Generic;

namespace Astruk.Services.Helpers
{
    class CoordsConstraints
    {


        public CoordsConstraints(IList<Vertex> Vertices)
        {
            XMin = double.MaxValue;
            XMax = -double.MaxValue;
            YMin = double.MaxValue;
            YMax = -double.MaxValue;

            foreach (var vertex in Vertices) {
                if (vertex.X > XMax) {
                    XMax = vertex.X;
                }

                if (vertex.X < XMin) {
                    XMin = vertex.X;
                }

                if (vertex.Y > YMax) {
                    YMax = vertex.Y;
                }

                if (vertex.Y < YMin) {
                    YMin = vertex.Y;
                }
            }
            XMax += 10;
            XMin -= 10;
            YMax += 10;
            YMin -= 10;
        }
        public CoordsConstraints(double XMin, double XMax, double YMin, double YMax)
        {
            this.XMin = XMin;
            this.XMax = XMax;
            this.YMin = YMin;
            this.YMax = YMax;
        }

        public double XMin { get; }
        public double XMax { get; }
        public double YMin { get; }
        public double YMax { get; }
    }
}
