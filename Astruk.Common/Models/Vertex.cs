using System.Diagnostics;

namespace Astruk.Common.Models
{
    [DebuggerDisplay("{ToString()}")]
    public class Vertex : Vector
    {
        public Vertex(double X, double Y) 
            : base(X, Y)
        {
            //Id = id;
            /*
            this.X = X;
            this.Y = Y;
            */
        }

        public int Id;// { get; }

        public override string ToString()
        {
            return $"v:{Id}";
        }
    }
}
