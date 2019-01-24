using Astruk.Common.Models;
using System;
using System.Collections.Generic;

namespace Astruk.Services.Models
{
    public class DeluanVertex : Vector
    {
        public DeluanVertex(double X, double Y, KeyMapObject KeyObject)
           : base(X, Y)
        {
            this.KeyObject = KeyObject;
            AdjacentTriangles = new List<Triangle>();
            ExoTriangles = new List<Triangle>();
            VoronoiVertices = new List<Vector>();
            Objects = new List<MapObject>();
        }

        public string Id { get; set; }
        public KeyMapObject KeyObject { get; }
        public List<Triangle> AdjacentTriangles { get; }
        public List<Triangle> ExoTriangles { get; }
        public List<Vector> VoronoiVertices { get; set; }
        public List<MapObject> Objects { get; }

        public bool isExo = false;


        public List<Vertex> GetVoronoiVerticesAsVertex()
        {
            var newList = new List<Vertex>(VoronoiVertices.Count);
            for(int i = 0; i < VoronoiVertices.Count; i++) {
                var vertex = VoronoiVertices[i];
                newList.Add(new Vertex(vertex.X, vertex.Y));
            }
            return newList;

        }
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
