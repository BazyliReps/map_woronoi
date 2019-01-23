using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astruk.Common.Models
{
    [DebuggerDisplay("{ToString()}")]
    public class Vector
    {
        public double X { get; }
        public double Y { get; }

        public Vector(double X, double Y)
        {
            this.X = X;
            this.Y = Y;
        }

        public Vector(Vector v)
        {
            this.X = v.X;
            this.Y = v.Y;
        }

        public static Vector operator -(Vector v1, Vector v2)
        {
            return new Vector(v1.X - v2.X, v1.Y - v2.Y);
        }

        public static double operator *(Vector v1, Vector v2)
        {
            return v1.X * v2.Y - v2.X * v1.Y;
        }

        public static Vector operator *(Vector v1, double t)
        {
            return new Vector(v1.X * t, v1.Y * t);
        }

        public static Vector operator *(double t, Vector v1)
        {
            return new Vector(v1.X * t, v1.Y * t);
        }

        public static Vector operator +(Vector v1, Vector v2)
        {
            return new Vector(v1.X + v2.X, v1.Y + v2.Y);
        }

        public double Magnitude()
        {
            return Math.Sqrt(X * X + Y * Y);
        }

        public override string ToString()
        {
            return $"x:{X}, y: {Y}";
        }

    }
}
