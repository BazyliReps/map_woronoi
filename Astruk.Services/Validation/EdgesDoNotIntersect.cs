using System;
using System.Collections.Generic;
using System.Linq;
using Astruk.Common.Interfaces;
using Astruk.Common.Models;

namespace Astruk.Services.Validation
{
	internal class EdgesDoNotIntersect : IValidationRule<IList<Vertex>>
	{
		private (Segment first, Segment second) IntersectingEdges { get; set; }

		public string ErrorMessage =>
			$"Krawędzie {IntersectingEdges.first} oraz {IntersectingEdges.second} przecinają się";

		public string ErrorKey => "vertices";
		public bool IsCritical => true;

		public bool IsValid(IList<Vertex> obj)
		{
			var edges = new List<Segment>();
			for (var i = 1; i < obj.Count; i++)
			{
				edges.Add(new Segment(obj[i - 1], obj[i]));
			}

			edges.Add(new Segment(obj.Last(), obj.First()));

			for (var i = 0; i < edges.Count; i++)
			{
				for (var j = i+1; j < edges.Count; j++)
				{
					if (!DoIntersect(edges[i], edges[j])) continue;
					IntersectingEdges = (edges[i], edges[j]);
					return false;
				}
			}

			return true;
		}

		private bool DoIntersect(Segment first, Segment second)
		{
			var o1 = OrderedTripletOrientation(first.From, first.To, second.From);
			var o2 = OrderedTripletOrientation(first.From, first.To, second.To);
			var o3 = OrderedTripletOrientation(second.From, second.To, first.From);
			var o4 = OrderedTripletOrientation(second.From, second.To, first.To);

			if (o1 != o2 && o3 != o4) return true;

			return (o1 == Orientation.Colinear && first.ContainsVertex(second.From))
			       || (o2 == Orientation.Colinear && first.ContainsVertex(second.To))
			       || (o3 == Orientation.Colinear && second.ContainsVertex(first.From))
			       || (o4 == Orientation.Colinear && second.ContainsVertex(first.To));
		}

		private Orientation OrderedTripletOrientation(Vertex p, Vertex q, Vertex r)
		{
			var val = (q.Y - p.Y) * (r.X - q.X) - (q.X - p.X) * (r.Y - q.Y);
			if (Math.Abs(val) < 0.000000001) return Orientation.Colinear; // Precision to 9 digits ought to be enough
			return val > 0 ? Orientation.Clockwise : Orientation.Counterclockwise;
		}

		internal class Segment
		{
			public Segment(Vertex from, Vertex to)
			{
				From = from;
				To = to;
			}

			public Vertex From { get; }
			public Vertex To { get; }

			public override string ToString()
			{
				return $"(X:{From.X} Y:{From.Y}, X:{To.X} Y:{To.Y})";
			}

			public bool ContainsVertex(Vertex vertex)
			{
				return vertex.X <= Math.Max(From.X, To.X)
				       && vertex.X >= Math.Min(From.X, To.X)
				       && vertex.X <= Math.Max(From.Y, To.Y)
				       && vertex.Y >= Math.Min(From.Y, To.Y);
			}
		}

		private enum Orientation
		{
			Colinear,
			Clockwise,
			Counterclockwise
		}
	}
}