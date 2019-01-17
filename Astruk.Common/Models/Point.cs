using System;
using System.Collections.Generic;

namespace Astruk.Common.Models
{
    public class Point
    {
        public Point(double x, double y)
        {
            Random r = new Random();
            this.X = x + r.NextDouble() * 0.00001;
            this.Y = y + r.NextDouble() * 0.00001;
        }

        public string Id { get; set; }
        public double X { get; }
        public double Y { get; }

        public List<Triangle> adjacentTriangles = new List<Triangle>();
        public List<Triangle> exoTriangles = new List<Triangle>();
        public List<Point> voronoiVertices = new List<Point>();

        public override string ToString()
        {
            return $"{Id}";
        }

        public override bool Equals(object obj)
        {
            Point p = (Point)obj;
            return this.X == p.X && this.Y == p.Y;
        }

        public override int GetHashCode()
        {
            var hashCode = 1426304211;
            hashCode = hashCode * -1521134295 + X.GetHashCode();
            hashCode = hashCode * -1521134295 + Y.GetHashCode();
            return hashCode;
        }
    }
}
