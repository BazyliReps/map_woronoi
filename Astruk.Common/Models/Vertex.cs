using System.Diagnostics;

namespace Astruk.Common.Models
{
    [DebuggerDisplay("{ToString()}")]
    public class Vertex : Vector
    {
        public Vertex(double X, double Y) 
            : base(X, Y)
        {
            
        }

        
    }
}
