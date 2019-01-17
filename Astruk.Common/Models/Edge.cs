using System.Collections.Generic;

namespace Astruk.Common.Models
{
    public class Edge
    {
        public Point start;
        public Point end;
        public Triangle baseTriangle;

        public Edge(Point start, Point end)
        {
            this.start = start;
            this.end = end;
        }

        public override string ToString()
        {
            return $"({start},{end})";
        }
    }
}
