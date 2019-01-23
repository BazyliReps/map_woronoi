using Astruk.Common.Models;

namespace Astruk.Common.Models
{
    public class BorderIntersectionEdge
    {
        public BorderIntersectionEdge(Vector start, Vector end, Vertex startVertexOfIntersectedEdgeIndex)
        {
            this.Start = start;
            this.End = end;
            this.StartVertexOfIntersectedEdge = startVertexOfIntersectedEdgeIndex;
        }

        public Vector Start { get; }
        public Vector End { get; }
        public Vertex StartVertexOfIntersectedEdge { get; }
    }
}
