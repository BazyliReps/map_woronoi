using Astruk.Services.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Astruk.Services.Helpers
{
	internal class Triangulator
	{
		public TriangulatedArea Triangulate(IEnumerable<DeluanVertex> keyObjects)
		{
			var hull = new LinkedList<Edge>();
			var triangles = new List<Triangle>(100);
			var points = keyObjects.OrderBy(v => v.X).ToList();

			InitPoints(points);

			InitializeHull(points, hull, triangles);

			for (var i = 3; i < points.Count; i++)
			{
				var currentPoint = points[i];
				var head = hull.First;
				while (head != null)
				{
					var madeTriangle = false;
					var currentEdge = head.Value;
					if (IsOnRight(currentEdge, currentPoint))
					{
						var newTriangle = new Triangle(currentEdge.end, currentEdge.start, currentPoint);
						triangles.Add(newTriangle);
						newTriangle.AddNeighbour(currentEdge.baseTriangle);
						currentEdge.baseTriangle.AddNeighbour(newTriangle);
						madeTriangle = true;
						var lowerEdge = new Edge(currentEdge.start, currentPoint);
						var upperEdge = new Edge(currentPoint, currentEdge.end);
						var previousEdgeNode = head.Previous ?? head.List.Last;
						var nextEdgeNode = head.Next ?? head.List.First;

						if (AreReversedEdges(previousEdgeNode.Value, lowerEdge))
						{
							previousEdgeNode.Value.baseTriangle.AddNeighbour(newTriangle);
							newTriangle.AddNeighbour(previousEdgeNode.Value.baseTriangle);
							hull.Remove(previousEdgeNode);
						}
						else
						{
							lowerEdge.baseTriangle = newTriangle;
							hull.AddBefore(head, lowerEdge);
						}

						if (AreReversedEdges(nextEdgeNode.Value, upperEdge))
						{
							nextEdgeNode.Value.baseTriangle.AddNeighbour(newTriangle);
							newTriangle.AddNeighbour(nextEdgeNode.Value.baseTriangle);
							hull.Remove(nextEdgeNode);
						}
						else
						{
							upperEdge.baseTriangle = newTriangle;
							hull.AddAfter(head, upperEdge);
						}

						LegalizeEdges(triangles, newTriangle, hull);
					}

					var nextEdge = head.Next;
					if (madeTriangle)
					{
						hull.Remove(head);
					}

					head = nextEdge;
				}
			}

			return new TriangulatedArea(triangles, points);
		}

		private static void LegalizeEdges(IList<Triangle> triangles, Triangle legalisedTriangle, LinkedList<Edge> hull)
		{
			var legalisationQueue = new Queue<Triangle>(100);
			legalisationQueue.Enqueue(legalisedTriangle);
			var ignoreList = new List<Triangle>();

			while (legalisationQueue.Count != 0)
			{
				var currentTriangle = legalisationQueue.Dequeue();
				if (ignoreList.Contains(currentTriangle))
				{
					ignoreList.Remove(currentTriangle);
					continue;
				}

				for (var i = 0; i < 3; i++)
				{
					var adjacentTriangle = currentTriangle.triangleNeighbours[i];
					if (adjacentTriangle == null) continue;
					var currentTriangleVertexIndex =
						currentTriangle.GetVertexOpposingGivenTriangle(adjacentTriangle);
					var adjacentTriangleVertexIndex =
						adjacentTriangle.GetVertexOpposingGivenTriangle(currentTriangle);
					var innerAngle = (currentTriangle.GetAngleOnVertex(currentTriangleVertexIndex)
					                  + adjacentTriangle.GetAngleOnVertex(adjacentTriangleVertexIndex));
					if (!(innerAngle > 180)) continue;
					var newTriangles = SwapEdgesAndFixNeighborRef(currentTriangle, adjacentTriangle,
						currentTriangle.points[currentTriangleVertexIndex]
						, adjacentTriangle.points[adjacentTriangleVertexIndex]);
					var firstNew = newTriangles[0];
					var secondNew = newTriangles[1];
					triangles.Remove(currentTriangle);
					triangles.Remove(adjacentTriangle);
					triangles.Add(newTriangles[0]);
					triangles.Add(newTriangles[1]);

					if (legalisationQueue.Contains(adjacentTriangle))
					{
						ignoreList.Add(adjacentTriangle);
					}

					foreach (var edge in hull)
					{
						if (firstNew.points.Contains(edge.start) && firstNew.points.Contains(edge.end))
						{
							edge.baseTriangle = firstNew;
						}
						else if (secondNew.points.Contains(edge.start) && secondNew.points.Contains(edge.end))
						{
							edge.baseTriangle = secondNew;
						}
					}

					foreach (var neighbor in firstNew.triangleNeighbours)
					{
						if (neighbor != secondNew && neighbor != null)
						{
							legalisationQueue.Enqueue(neighbor);
						}
					}

					foreach (var neighbor in secondNew.triangleNeighbours)
					{
						if (neighbor != firstNew && neighbor != null)
						{
							legalisationQueue.Enqueue(neighbor);
						}
					}
				}
			}
		}

		private static List<Triangle> SwapEdgesAndFixNeighborRef(Triangle swappedTriangle, Triangle adjacentTriangle,
			DeluanVertex swappedVertex, DeluanVertex adjacentVertex)
		{
			var newTriangles = new List<Triangle>(2);

			var commonVertices = swappedTriangle.points.Where(p => p != swappedVertex).ToList();
			var firstTriangleVertices = new List<DeluanVertex>
			{
				swappedVertex,
				adjacentVertex,
				commonVertices[0]
			};

			var secondTriangleVertices = new List<DeluanVertex>
			{
				swappedVertex,
				adjacentVertex,
				commonVertices[1]
			};

			firstTriangleVertices.Sort((p1, p2) => p1.X.CompareTo(p2.X));
			secondTriangleVertices.Sort((p1, p2) => p1.X.CompareTo(p2.X));

			var first = MakeTriangleFromPoints(firstTriangleVertices);
			var second = MakeTriangleFromPoints(secondTriangleVertices);

			var firstNeighborIndex = GetNeighbourIndexByVertex(commonVertices[0], first.points);
			var secondNeighborIndex = GetNeighbourIndexByVertex(commonVertices[1], second.points);

			first.triangleNeighbours[firstNeighborIndex] = second;
			second.triangleNeighbours[secondNeighborIndex] = first;

			FixNeighbors(swappedTriangle, adjacentTriangle, first, second);

			FixNeighbors(adjacentTriangle, swappedTriangle, first, second);

			newTriangles.Add(first);
			newTriangles.Add(second);

			return newTriangles;
		}

		private static Triangle MakeTriangleFromPoints(IList<DeluanVertex> points)
		{
			var p0 = points[0];
			var p1 = points[1];
			var p2 = points[2];
			var edge0 = new Edge(p0, p1);

			var firstTriangle = !IsOnRight(edge0, p2) ? new Triangle(p0, p1, p2) : new Triangle(p1, p0, p2);

			return firstTriangle;
		}

		private static void FixNeighbors(Triangle beforeSwap1, Triangle beforeSwap2, Triangle afterSwap1,
			Triangle afterSwap2)
		{
			foreach (var neighbor in beforeSwap1.triangleNeighbours)
			{
				if (neighbor == beforeSwap2)
				{
					continue;
				}

				if (CheckIfNeighbors(neighbor, afterSwap1))
				{
					neighbor.AddNeighbour(afterSwap1);
					afterSwap1.AddNeighbour(neighbor);
				}

				if (!CheckIfNeighbors(neighbor, afterSwap2)) continue;
				neighbor.AddNeighbour(afterSwap2);
				afterSwap2.AddNeighbour(neighbor);
			}
		}

		private static int GetNeighbourIndexByVertex(DeluanVertex vertex, DeluanVertex[] vertices)
		{
			var index = Array.IndexOf(vertices, vertex);
			return index == 2 ? 0 : index + 1;
		}

		private static bool CheckIfNeighbors(Triangle t1, Triangle t2)
		{
			if (t1 == null || t2 == null)
			{
				return false;
			}

			return t1.points.Intersect(t2.points).Count() == 2;
		}

		private static void InitPoints(IList<DeluanVertex> p)
		{
			var index = 0;
			foreach (var point in p)
			{
				point.Id = "p" + index++;
			}
		}

		private static bool AreReversedEdges(Edge e1, Edge e2)
		{
			return e1.start == e2.end && e1.end == e2.start;
		}

		private static bool IsOnRight(Edge edge, DeluanVertex checkedPoint)
		{
			return (checkedPoint.X - edge.start.X) * (edge.end.Y - edge.start.Y) -
			       (edge.end.X - edge.start.X) * (checkedPoint.Y - edge.start.Y) < 0;
		}

		private static void InitializeHull(IList<DeluanVertex> points, LinkedList<Edge> hull, IList<Triangle> triangles)
		{
			var firstTriangle = MakeTriangleFromPoints(points);

			var edge0 = new Edge(firstTriangle.points[0], firstTriangle.points[1]);
			var edge1 = new Edge(firstTriangle.points[1], firstTriangle.points[2]);
			var edge2 = new Edge(firstTriangle.points[2], firstTriangle.points[0]);

			edge0.baseTriangle = firstTriangle;
			edge1.baseTriangle = firstTriangle;
			edge2.baseTriangle = firstTriangle;

			triangles.Add(firstTriangle);
			hull.AddLast(edge0);
			hull.AddLast(edge1);
			hull.AddLast(edge2);
		}
	}
}