using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using Astruk.Common.Models;

namespace Astruk.Services.Models
{
    [DebuggerDisplay("{ToString()}")]
    public class Triangle
    {
        public Triangle[] triangleNeighbours;
        public DeluanVertex[] points;
        public Vector circumcenter;
        public bool isObtuse = false;
        public List<Vector> intersections = new List<Vector>();

        private readonly double[] angles;

        public Triangle(DeluanVertex p1, DeluanVertex p2, DeluanVertex p3)
        {
            this.triangleNeighbours = new Triangle[3];
            this.points = new DeluanVertex[3];
            this.angles = new double[3];
            points[0] = p1;
            points[1] = p2;
            points[2] = p3;

            FindCircumcenter();

            for (int i = 0; i < 3; i++) {
                angles[i] = CalculateAngleOnVertex(i);
                if(angles[i] > 90) {
                    isObtuse = true;
                }
            }
        }

        public void AddNeighbour(Triangle neighbour)
        {
            var sharedVertices = points.Intersect(neighbour.points);
            int index;
            if (!sharedVertices.Contains(points[2])) {
                index = 0;
            } else if (!sharedVertices.Contains(points[0])) {
                index = 1;
            } else {
                index = 2;
            }
            triangleNeighbours[index] = neighbour;
        }

        public int GetVertexOpposingGivenTriangle(Triangle adjacentTriangle)
        {
            int index = Array.IndexOf(triangleNeighbours, adjacentTriangle);
            return index - 1 < 0 ? 2 : index - 1;
        }

        public double GetAngleOnVertex(int vertexId)
        {
            return angles[vertexId];
        }

        private double CalculateAngleOnVertex(int startVertexIndex)
        {

            int end1, end2;

            end1 = startVertexIndex + 1;
            end2 = startVertexIndex + 2;

            end1 = end1 > 2 ? end1 % 3 : end1;
            end2 = end2 > 2 ? end2 % 3 : end2;

            var angle = Math.Atan2(points[end2].Y - points[startVertexIndex].Y, points[end2].X - points[startVertexIndex].X)
               - Math.Atan2(points[end1].Y - points[startVertexIndex].Y, points[end1].X - points[startVertexIndex].X);

            var ret = Math.Abs(angle * (180 / Math.PI));
            ret = ret > 180 ? 360 - ret : ret;
            return ret;
        }

        private void FindCircumcenter()
        {
            var p1 = this.points[0];
            var p2 = this.points[1];
            var p3 = this.points[2];

            double A = p2.X - p1.X,
             B = p2.Y - p1.Y,
             C = p3.X - p1.X,
             D = p3.Y - p1.Y,
             E = A * (p1.X + p2.X) + B * (p1.Y + p2.Y),
             F = C * (p1.X + p3.X) + D * (p1.Y + p3.Y),
             G = 2 * (A * (p3.Y - p2.Y) - B * (p3.X - p2.X));
            double x = (D * E - B * F) / G;
            double y = (A * F - C * E) / G;

            this.circumcenter = new Vector(x, y);

        }

        public bool ContainsPoint(DeluanVertex p)
        {
            return points.Contains(p);
        }


        public override string ToString()
        {
            return $"{points[0]}, {points[1]}, {points[2]}";
        }

    }
}
