using System;
using System.Collections.Generic;

namespace Astruk.Common.Models
{
    public class DeluanVertex : Vector
    {

        public DeluanVertex(double X, double Y)
           : base(X, Y)
        {

        }

        public string Id { get; set; }


        public List<Triangle> adjacentTriangles = new List<Triangle>();
        public List<Triangle> exoTriangles = new List<Triangle>();
        public List<Vector> voronoiVertices = new List<Vector>();

        public bool isExo = false;


        public override string ToString()
        {
            return $"{Id}";
        }

        public override bool Equals(object obj)
        {
            DeluanVertex p = (DeluanVertex)obj;
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
