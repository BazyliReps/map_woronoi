using System;
using System.Collections.Generic;
using System.Linq;
using Astruk.Common.Interfaces;
using Astruk.Common.Models;
using Astruk.Services.Helpers;
using Astruk.Services.Models;

namespace Astruk.Services
{
	internal class MapService : IMapService
	{
		public Map GenerateMap(IList<Vertex> vertices, IEnumerable<KeyMapObject> keyObjects,
			IEnumerable<MapObjectType> types, IEnumerable<MapObject> objects)
		{
			SetBordersClockwise(ref vertices);
			var triangulationMaker = new Triangulator();
			var mapConstraints = new CoordsConstraints(vertices);
			var deluanVertices = keyObjects.Select(keyObject => new DeluanVertex(keyObject.X, keyObject.Y, keyObject))
				.ToList();
			var triangulatedMap = triangulationMaker.Triangulate(deluanVertices);

			InitVoronoiCreation(triangulatedMap.triangles);
			GetVoronoiVertices(deluanVertices, vertices, mapConstraints);
			AssignObjectsToDeluanVertices(objects, deluanVertices);

			return new Map(vertices, CreateRegions(deluanVertices));
		}

		private IEnumerable<Region> CreateRegions(IList<DeluanVertex> deluanVertices)
		{
			return deluanVertices.Select(deluanVertex => new Region(deluanVertex.KeyObject,
				deluanVertex.GetVoronoiVerticesAsVertex(), deluanVertex.Objects)).ToList();
		}

		private static void AssignObjectsToDeluanVertices(IEnumerable<MapObject> objects,
			IList<DeluanVertex> regionCenters)
		{
			foreach (var currentObject in objects)
			{
				DeluanVertex closestRegion = null;
				var minDistance = double.MaxValue;
				foreach (var region in regionCenters)
				{
					if (!currentObject.Parameters.TryGetValue("x", out var xString))
					{
						currentObject.Parameters.TryGetValue("X", out xString);
					}

					if (!currentObject.Parameters.TryGetValue("y", out var yString))
					{
						currentObject.Parameters.TryGetValue("Y", out yString);
					}

					if (double.TryParse(xString, out var x) && double.TryParse(yString, out var y))
					{
						var distance = Math.Sqrt((region.X - x) * (region.X - x) + (region.Y - x) * (region.Y - y));
						if (!(distance < minDistance)) continue;
						minDistance = distance;
						closestRegion = region;
					}
				}

				closestRegion?.Objects.Add(currentObject);
			}
		}

		private void SetBordersClockwise(ref IList<Vertex> vertices)
		{
			double det = 0;
			for (var i = 0; i < vertices.Count - 1; i++)
			{
				var v1 = vertices[i];
				var v2 = vertices[i + 1];
				det += (v2.X - v1.X) * (v2.Y + v1.Y);
			}

			if (!(det > 0)) return;
			var newList = new List<Vertex>();
			for (var i = 0; i < vertices.Count; i++)
			{
				newList.Add(vertices[vertices.Count - i - 1]);
			}

			vertices = newList;
		}

		private void InitVoronoiCreation(IList<Triangle> allTriangles)
		{
			foreach (var triangle in allTriangles)
			{
				for (var i = 0; i < 3; i++)
				{
					var vertex = triangle.points[i];
					var prevIndex = i == 0 ? 2 : i - 1;
					if (triangle.triangleNeighbours[i] == null || triangle.triangleNeighbours[prevIndex] == null)
					{
						vertex.IsExo = true;
						vertex.ExoTriangles.Add(triangle);
					}

					vertex.AdjacentTriangles.Add(triangle);
				}
			}
		}

		private void GetVoronoiVertices(List<DeluanVertex> allPoints, IList<Vertex> vertices,
			CoordsConstraints mapConstraints)
		{
			var borders = new List<Vector>()
			{
				new Vector(mapConstraints.XMin, mapConstraints.YMin),
				new Vector(mapConstraints.XMax, mapConstraints.YMin),
				new Vector(mapConstraints.XMax, mapConstraints.YMax),
				new Vector(mapConstraints.XMin, mapConstraints.YMax)
			};
			foreach (var deluanVertex in allPoints)
			{
				var shouldCheckEdges = false;
				if (deluanVertex.ExoTriangles.Count == 0)
				{
					var firstTriangle = deluanVertex.AdjacentTriangles.First();
					var currentTriangle = firstTriangle;
					var numberOfTriangles = deluanVertex.AdjacentTriangles.Count;

					for (var j = 0; j < numberOfTriangles; j++)
					{
						if (currentTriangle.isObtuse)
						{
							shouldCheckEdges = true;
						}

						deluanVertex.VoronoiVertices.Add(currentTriangle.circumcenter);
						for (var i = 0; i < 3; i++)
						{
							var nextTriangle = currentTriangle.triangleNeighbours[i];
							if (nextTriangle == null || i != Array.IndexOf(currentTriangle.points, deluanVertex))
								continue;
							currentTriangle = nextTriangle;
							break;
						}
					}

					if (shouldCheckEdges)
					{
						CheckEdges(deluanVertex, vertices, false);
					}
				}
				else
				{
					Vector intersection1 = null,
						intersection2 = null;
					var firstIntersectedEdgeId = -1;
					var secondIntersectedEdgeId = -1;
					var exoTriangle = deluanVertex.ExoTriangles.First();
					var endTriangle = deluanVertex.ExoTriangles.Last();
					var isClockwise = false;
					var foundFirstExoLine = false;

					if (deluanVertex.ExoTriangles.Count == 1)
					{
						secondIntersectedEdgeId = Array.IndexOf(exoTriangle.points, deluanVertex);
						firstIntersectedEdgeId = secondIntersectedEdgeId == 0 ? 2 : secondIntersectedEdgeId - 1;
						if (IsWithinBorders(exoTriangle.circumcenter, mapConstraints))
						{
							MakeExoEdge(exoTriangle, borders, firstIntersectedEdgeId, ref intersection1,
								ref firstIntersectedEdgeId, mapConstraints);
							MakeExoEdge(exoTriangle, borders, secondIntersectedEdgeId, ref intersection2,
								ref secondIntersectedEdgeId, mapConstraints);
							deluanVertex.VoronoiVertices.Add(intersection1);
							deluanVertex.VoronoiVertices.Add(exoTriangle.circumcenter);
							deluanVertex.VoronoiVertices.Add(intersection2);
						}
					}
					else
					{
						for (var i = 0; i < 3; i++)
						{
							var neighbor = exoTriangle.triangleNeighbours[i];
							if (neighbor == null || Array.IndexOf(exoTriangle.points, deluanVertex) != i) continue;
							isClockwise = true;
							break;
						}

						if (!isClockwise)
						{
							exoTriangle = deluanVertex.ExoTriangles.Last();
							endTriangle = deluanVertex.ExoTriangles.First();
						}

						var nextTriangleIndex = Array.IndexOf(exoTriangle.points, deluanVertex);
						var nullNeighborIndex = nextTriangleIndex - 1;
						nullNeighborIndex = nullNeighborIndex < 0 ? 2 : nullNeighborIndex;

						if (IsWithinBorders(exoTriangle.circumcenter, mapConstraints))
						{
							if (MakeExoEdge(exoTriangle, borders, nullNeighborIndex, ref intersection1,
								ref firstIntersectedEdgeId, mapConstraints))
							{
								deluanVertex.VoronoiVertices.Add(intersection1);
								foundFirstExoLine = true;
							}
						}

						var currentTriangle = exoTriangle;
						var nextTriangle = currentTriangle;
						bool isOnRight;

						for (var j = 0; j < deluanVertex.AdjacentTriangles.Count; j++)
						{
							for (var i = 0; i < 3; i++)
							{
								nextTriangle = currentTriangle?.triangleNeighbours[i];
								if (nextTriangle != null && i == Array.IndexOf(currentTriangle.points, deluanVertex))
								{
									break;
								}
							}

							if (IsWithinBorders(currentTriangle?.circumcenter, mapConstraints))
							{
								deluanVertex.VoronoiVertices.Add(currentTriangle?.circumcenter);
							}
							else
							{
								for (var i = 0; i < borders.Count; i++)
								{
									if (foundFirstExoLine)
									{
										if (!CheckIfLineSegmentsIntersects(currentTriangle?.circumcenter,
											deluanVertex.VoronoiVertices.Last(), borders[i],
											borders[i == 3 ? 0 : i + 1],
											ref intersection2, out isOnRight)) continue;
										deluanVertex.VoronoiVertices.Add(new Vector(intersection2));
										secondIntersectedEdgeId = i;
										break;
									}

									if (currentTriangle == endTriangle) continue;
									if (!CheckIfLineSegmentsIntersects(currentTriangle?.circumcenter,
										nextTriangle?.circumcenter, borders[i], borders[i == 3 ? 0 : i + 1],
										ref intersection1, out isOnRight)) continue;
									deluanVertex.VoronoiVertices.Add(new Vector(intersection1));
									firstIntersectedEdgeId = i;
									foundFirstExoLine = true;
									break;
								}
							}

							currentTriangle = nextTriangle;
						}

						nullNeighborIndex = Array.IndexOf(endTriangle.points, deluanVertex);
						if (IsWithinBorders(endTriangle.circumcenter, mapConstraints))
						{
							if (MakeExoEdge(endTriangle, borders, nullNeighborIndex, ref intersection2,
								ref secondIntersectedEdgeId, mapConstraints))
							{
								deluanVertex.VoronoiVertices.Add(new Vector(intersection2));
							}
						}
					}

					var currentId = secondIntersectedEdgeId;
					if (firstIntersectedEdgeId != secondIntersectedEdgeId)
					{
						do
						{
							currentId = currentId == 3 ? 0 : currentId + 1;
							deluanVertex.VoronoiVertices.Add(borders[currentId]);
						} while (currentId != firstIntersectedEdgeId);
					}

					CheckEdges(deluanVertex, vertices, true);
				}
			}
		}

		private bool IsWithinBorders(Vector v, CoordsConstraints constraints)
		{
			return (v.X > constraints.XMin && v.X < constraints.XMax && v.Y > constraints.YMin &&
			        v.Y < constraints.YMax);
		}


		private bool MakeExoEdge(Triangle triangle, List<Vector> borderVertices, int nullNeighborIndex,
			ref Vector intersection, ref int edgeId, CoordsConstraints mapConstraints)
		{
			Edge exoTriangleEdge;

			switch (nullNeighborIndex)
			{
				case 0:
					exoTriangleEdge = new Edge(triangle.points[0], triangle.points[1]);
					break;
				case 1:
					exoTriangleEdge = new Edge(triangle.points[1], triangle.points[2]);
					break;
				default:
					exoTriangleEdge = new Edge(triangle.points[2], triangle.points[0]);
					break;
			}

			var normalPoint = new Vector((exoTriangleEdge.start.X + exoTriangleEdge.end.X) / 2,
				(exoTriangleEdge.start.Y + exoTriangleEdge.end.Y) / 2);
			var constraints = MakeLineConstraints(triangle, normalPoint, nullNeighborIndex);

			for (var j = 0; j < borderVertices.Count; j++)
			{
				var startBorderVertex = borderVertices[j];
				var endBorderVertex = j == borderVertices.Count - 1 ? borderVertices[0] : borderVertices[j + 1];
				Vector normalLineAndBorderIntersection = null;
				try
				{
					normalLineAndBorderIntersection = GetIntersectionPoint(triangle.circumcenter, normalPoint,
						startBorderVertex, endBorderVertex);
				}
				catch (ArgumentException)
				{
					continue;
				}

				if (!IsPointInDomain(normalLineAndBorderIntersection, startBorderVertex, endBorderVertex,
					constraints)) continue;
				intersection = normalLineAndBorderIntersection;
				edgeId = j;
				return true;
			}

			return false;
		}


		private void CheckEdges(DeluanVertex point, IList<Vertex> vertices, bool isOuterRegion)
		{
			Vector intersection = null;
			var intersectionsFound = 0;
			var outsideBorders = false;

			var toRemove = new List<Vector>();
			var foundIntersections = new List<Vector>();
			var innerVoronoiVertices = new List<Vector>();
			var idsOfEdges = new List<int>();
			var suspectedOfBeingOutside = new List<Vector>();

			for (var i = 0; i < point.VoronoiVertices.Count; i++)
			{
				var currentVoronoiVertex = point.VoronoiVertices[i];
				var nextI = i == point.VoronoiVertices.Count - 1 ? 0 : i + 1;
				var nextVoronoiVertex = point.VoronoiVertices[nextI];
				var timesFirstInside = 0;
				var timesSecondInside = 0;

				if (outsideBorders)
				{
					toRemove.Add(currentVoronoiVertex);
				}

				for (var j = 0; j < vertices.Count; j++)
				{
					Vector v1 = vertices[j];
					var nextJ = j == vertices.Count - 1 ? 0 : j + 1;
					Vector v2 = vertices[nextJ];

					if (!CheckIfLineSegmentsIntersects(currentVoronoiVertex, nextVoronoiVertex, v1, v2,
						ref intersection, out var isFirstInside)) continue;
					intersectionsFound++;
					idsOfEdges.Add(j);
					if (isFirstInside)
					{
						if (foundIntersections.Count == 0)
						{
							suspectedOfBeingOutside.Clear();
						}

						innerVoronoiVertices.Add(currentVoronoiVertex);
						outsideBorders = true;
						timesFirstInside++;
					}
					else
					{
						innerVoronoiVertices.Add(nextVoronoiVertex);
						if (!toRemove.Contains(currentVoronoiVertex))
						{
							toRemove.Add(currentVoronoiVertex);
						}

						outsideBorders = false;
						timesSecondInside++;
					}

					foundIntersections.Add(new Vector(intersection));
				}

				if (timesFirstInside > timesSecondInside)
				{
					toRemove.Remove(currentVoronoiVertex);
				}

				if (foundIntersections.Count == 0)
				{
					suspectedOfBeingOutside.Add(currentVoronoiVertex);
				}
			}

			if (foundIntersections.Count == 0)
			{
				suspectedOfBeingOutside.Clear();
			}

			toRemove.AddRange(suspectedOfBeingOutside);

			foreach (var v in toRemove)
			{
				point.VoronoiVertices.Remove(v);
			}

			if (foundIntersections.Count <= 0) return;
			for (var i = 0; i < foundIntersections.Count - 1; i += 2)
			{
				FixBorders(point, vertices, foundIntersections[i], foundIntersections[i + 1], idsOfEdges[i],
					idsOfEdges[i + 1], innerVoronoiVertices[i], innerVoronoiVertices[i + 1], isOuterRegion);
			}
		}

		private static bool FixBorders(DeluanVertex point, IList<Vertex> vertices, Vector intersection1,
			Vector intersection2,
			int edgeId1, int edgeId2, Vector firstInnerVoronoiVertex, Vector secondInnerVoronoiVertex,
			bool IsOuterRegion)
		{
			var newList = new List<Vector>();
			var i1 = intersection1;
			var i2 = intersection2;

			var firstInnerVoronoiVertexId = point.VoronoiVertices.IndexOf(firstInnerVoronoiVertex);
			var secondInnerVoronoiVertexId = point.VoronoiVertices.IndexOf(secondInnerVoronoiVertex);

			Vector firstCircumInside;
			Vector lastCircumInside;

			int edge1 = edgeId1,
				edge2 = edgeId2,
				v1 = firstInnerVoronoiVertexId,
				v2 = secondInnerVoronoiVertexId;

			if (edgeId1 > edgeId2 || (edgeId1 == edgeId2 && (intersection1 - vertices[edgeId1]).Magnitude() >
			                          (intersection2 - vertices[edgeId1]).Magnitude()))
			{
				i1 = intersection2;
				i2 = intersection1;
				edge1 = edgeId2;
				edge2 = edgeId1;
				v1 = secondInnerVoronoiVertexId;
				v2 = firstInnerVoronoiVertexId;
			}

			try
			{
				firstCircumInside = point.VoronoiVertices[v1];
				lastCircumInside = point.VoronoiVertices[v2];
			}
			catch (ArgumentOutOfRangeException)
			{
				return false;
			}

			var prevIndex = v1 - 1 < 0 ? point.VoronoiVertices.Count - 1 : v1 - 1;

			if (!CheckIfLineSegmentsIntersects(point, vertices[edge2], lastCircumInside, i2))
			{
				newList.Add(i1);
				for (var i = edge1 + 1; i <= edge2; i++)
				{
					newList.Add(vertices[i]);
				}

				newList.Add(i2);
				var currentVertex = lastCircumInside;
				var nextIndex = v2;
				newList.Add(currentVertex);
				while (currentVertex != firstCircumInside)
				{
					nextIndex = nextIndex == point.VoronoiVertices.Count - 1 ? 0 : nextIndex + 1;
					currentVertex = point.VoronoiVertices[nextIndex];
					newList.Add(currentVertex);
				}

				point.VoronoiVertices = newList;
				return true;
			}
			else
			{
				newList.Add(i2);
				var nextIndex = edge2;
				do
				{
					nextIndex = nextIndex + 1 == vertices.Count ? 0 : nextIndex + 1;
					newList.Add(vertices[nextIndex]);
				} while (nextIndex != edge1);

				newList.Add(i1);
				var currentVertex = firstCircumInside;
				nextIndex = v1;

				newList.Add(currentVertex);
				while (currentVertex != lastCircumInside)
				{
					nextIndex = nextIndex == point.VoronoiVertices.Count - 1 ? 0 : nextIndex + 1;
					currentVertex = point.VoronoiVertices[nextIndex];
					newList.Add(currentVertex);
				}

				point.VoronoiVertices = newList;
				return true;
			}
		}

		private bool CheckIfLineSegmentsIntersects(Vector start, Vector end, Vector edgeStart, Vector edgeEnd,
			ref Vector intersection, out bool firstOnRight)
		{
			const double nullOffset = 0.00000001;
			var r = end - start;
			var s = edgeEnd - edgeStart;
			var product = r * s;
			if (Math.Abs(product) > nullOffset)
			{
				var t = ((edgeStart - start) * s) / product;
				var u = ((edgeStart - start) * r) / product;
				if (t >= 0 && t <= 1 && u >= 0 && u <= 1)
				{
					intersection = start + t * r;
					firstOnRight = IsOnRight(edgeStart, edgeEnd, start);
					return true;
				}
			}

			firstOnRight = false;
			return false;
		}

		private static bool IsOnRight(Vector edgeVertex1, Vector edgeVertex2, Vector checkedPoint)
		{
			return (checkedPoint.X - edgeVertex1.X) * (edgeVertex2.Y - edgeVertex1.Y) -
			       (edgeVertex2.X - edgeVertex1.X) * (checkedPoint.Y - edgeVertex1.Y) < 0;
		}

		private static bool CheckIfLineSegmentsIntersects(Vector p, Vector pr, Vector q, Vector qs)
		{
			var nullOffset = 0.00000001;
			var r = pr - p;
			var s = qs - q;
			var product = r * s;
			if (!(Math.Abs(product) > nullOffset)) return false;
			var t = ((q - p) * s) / product;
			var u = ((q - p) * r) / product;
			return t >= 0 && t <= 1 && u >= 0 && u <= 1;
		}

		private CoordsConstraints MakeLineConstraints(Triangle triangle, Vector normalPoint, int vertexIndex)
		{
			double xMin;
			double xMax;
			double yMin;
			double yMax;

			var oppositeVertexId = vertexIndex == 0 ? 2 : vertexIndex - 1;
			var angle = triangle.GetAngleOnVertex(oppositeVertexId);
			var circumcenterOutsideTriangle = angle > 90;


			if (circumcenterOutsideTriangle)
			{
				if (normalPoint.X > triangle.circumcenter.X)
				{
					xMin = -double.MaxValue;
					xMax = triangle.circumcenter.X;
					if (normalPoint.Y > triangle.circumcenter.Y)
					{
						yMin = -double.MaxValue;
						yMax = triangle.circumcenter.Y;
					}
					else
					{
						yMin = triangle.circumcenter.Y;
						yMax = double.MaxValue;
					}
				}
				else
				{
					xMin = triangle.circumcenter.X;
					xMax = double.MaxValue;
					if (normalPoint.Y > triangle.circumcenter.Y)
					{
						yMin = -double.MaxValue;
						yMax = triangle.circumcenter.Y;
					}
					else
					{
						yMin = triangle.circumcenter.Y;
						yMax = double.MaxValue;
					}
				}
			}
			else
			{
				if (normalPoint.X > triangle.circumcenter.X)
				{
					xMin = triangle.circumcenter.X;
					xMax = double.MaxValue;
					if (normalPoint.Y > triangle.circumcenter.Y)
					{
						yMin = triangle.circumcenter.Y;
						yMax = double.MaxValue;
					}
					else
					{
						yMin = -double.MaxValue;
						yMax = triangle.circumcenter.Y;
					}
				}
				else
				{
					xMin = -double.MaxValue;
					xMax = triangle.circumcenter.X;
					if (normalPoint.Y > triangle.circumcenter.Y)
					{
						yMin = triangle.circumcenter.Y;
						yMax = double.MaxValue;
					}
					else
					{
						yMin = -double.MaxValue;
						yMax = triangle.circumcenter.Y;
					}
				}
			}

			return new CoordsConstraints(xMin, xMax, yMin, yMax);
		}

		private static bool IsPointInDomain(Vector intersectionPoint, Vector startBorderVertex, Vector endBorderVertex,
			CoordsConstraints c)
		{
			var firstLower = startBorderVertex.X < endBorderVertex.X;
			var xMin = firstLower ? startBorderVertex.X : endBorderVertex.X;
			var xMax = firstLower ? endBorderVertex.X : startBorderVertex.X;

			firstLower = startBorderVertex.Y < endBorderVertex.Y;
			var yMin = firstLower ? startBorderVertex.Y : endBorderVertex.Y;
			var yMax = firstLower ? endBorderVertex.Y : startBorderVertex.Y;

			var horizontalParallel = yMin == yMax;
			var verticalParallel = xMin == xMax;


			var isInDomain = intersectionPoint.X >= c.XMin
			                 && (intersectionPoint.X >= xMin || verticalParallel)
			                 && intersectionPoint.X <= c.XMax
			                 && (intersectionPoint.X <= xMax || verticalParallel)
			                 && intersectionPoint.Y >= c.YMin
			                 && (intersectionPoint.Y >= yMin || horizontalParallel)
			                 && intersectionPoint.Y <= c.YMax
			                 && (intersectionPoint.Y <= yMax || horizontalParallel);
			return isInDomain;
		}

		private Vector GetIntersectionPoint(Vector start1, Vector end1, Vector start2, Vector end2)
		{
			const double nullOffset = 0.00001;

			var a1 = end1.Y - start1.Y;
			var b1 = start1.X - end1.X;
			var c1 = start1.X * a1 + start1.Y * b1;

			var a2 = end2.Y - start2.Y;
			var b2 = start2.X - end2.X;
			var c2 = start2.X * a2 + start2.Y * b2;

			var det = a1 * b2 - a2 * b1;

			if (Math.Abs(det) < nullOffset)
			{
				throw new ArgumentException("linie równoległe");
			}

			var x = (b2 * c1 - b1 * c2) / det;
			var y = (a1 * c2 - a2 * c1) / det;

			return new Vector(x, y);
		}
	}
}