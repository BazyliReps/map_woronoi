using Astruk.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astruk.Services.Helpers
{
    class Triangulator
    {
        public TestMap Triangulate(IEnumerable<DeluanVertex> KeyObjects, IList<Vertex> Vertices)
        {
            var Hull = new LinkedList<Edge>();
            var Triangles = new List<Triangle>(100);
            var Points = KeyObjects.OrderBy(v => v.X).ToList();
            bool madeTriangle = false;

            

            InitPoints(Points);

            InitializeHull(Points, Hull, Triangles);

            for (int i = 3; i < Points.Count; i++) {
                DeluanVertex currentPoint = Points[i];
                LinkedListNode<Edge> head = Hull.First;
                while (head != null) {
                    madeTriangle = false;
                    Edge currentEdge = head.Value;
                    if (IsOnRight(currentEdge, currentPoint)) {
                        Triangle newTriangle = new Triangle(currentEdge.end, currentEdge.start, currentPoint);
                        Triangles.Add(newTriangle);

                        //dodaj sasiada z bazowej krawedzi
                        newTriangle.AddNeighbour(currentEdge.baseTriangle);
                        currentEdge.baseTriangle.AddNeighbour(newTriangle);

                        madeTriangle = true;

                        Edge lowerEdge = new Edge(currentEdge.start, currentPoint);
                        Edge upperEdge = new Edge(currentPoint, currentEdge.end);

                        var previousEdgeNode = head.Previous ?? head.List.Last;
                        var nextEdgeNode = head.Next ?? head.List.First;

                        //is lower reversed?
                        if (AreReversedEdges(previousEdgeNode.Value, lowerEdge)) {
                            previousEdgeNode.Value.baseTriangle.AddNeighbour(newTriangle);
                            newTriangle.AddNeighbour(previousEdgeNode.Value.baseTriangle);
                            Hull.Remove(previousEdgeNode);
                        } else {
                            lowerEdge.baseTriangle = newTriangle;
                            Hull.AddBefore(head, lowerEdge);
                        }

                        //is upper reversed?
                        if (AreReversedEdges(nextEdgeNode.Value, upperEdge)) {
                            nextEdgeNode.Value.baseTriangle.AddNeighbour(newTriangle);
                            newTriangle.AddNeighbour(nextEdgeNode.Value.baseTriangle);
                            Hull.Remove(nextEdgeNode);
                        } else {
                            upperEdge.baseTriangle = newTriangle;
                            Hull.AddAfter(head, upperEdge);
                        }
                        LegalizeEdges(Triangles, newTriangle, Hull);
                    }
                    var nextEdge = head.Next;
                    if (madeTriangle) {
                        Hull.Remove(head);
                    }
                    head = nextEdge;
                }
            }


            

            

            return new TestMap(Triangles, Points);
        }

        

        private void LegalizeEdges(List<Triangle> triangles, Triangle legalisedTriangle, LinkedList<Edge> hull)
        {
            var legalisationQueue = new Queue<Triangle>(100);
            legalisationQueue.Enqueue(legalisedTriangle);

            var ignoreList = new List<Triangle>();


            while (legalisationQueue.Count != 0) {
                Triangle currentTriangle = legalisationQueue.Dequeue();

                if (ignoreList.Contains(currentTriangle)) {
                    ignoreList.Remove(currentTriangle);
                    currentTriangle = null;
                    continue;
                }

                for (int i = 0; i < 3; i++) {
                    Triangle adjacentTriangle = currentTriangle.triangleNeighbours[i];
                    if (adjacentTriangle != null) {
                        int currentTriangleVertexIndex = currentTriangle.GetVertexOpposingGivenTriangle(adjacentTriangle);
                        int adjacentTriangleVertexIndex = adjacentTriangle.GetVertexOpposingGivenTriangle(currentTriangle);

                        double innerAngle = (currentTriangle.GetAngleOnVertex(currentTriangleVertexIndex)
                            + adjacentTriangle.GetAngleOnVertex(adjacentTriangleVertexIndex));

                        if (innerAngle > 180) {
                            //swap edges

                            var newTriangles = SwapEdgesAndFixNeighborRef(currentTriangle, adjacentTriangle, currentTriangle.points[currentTriangleVertexIndex]
                                , adjacentTriangle.points[adjacentTriangleVertexIndex]);
                            var firstNew = newTriangles[0];
                            var secondNew = newTriangles[1];

                            triangles.Remove(currentTriangle);
                            triangles.Remove(adjacentTriangle);

                            triangles.Add(newTriangles[0]);
                            triangles.Add(newTriangles[1]);

                            if (legalisationQueue.Contains(adjacentTriangle)) {
                                ignoreList.Add(adjacentTriangle);
                                adjacentTriangle = null;
                            }

                            //popraw na sprawdzenie tylko 4 ostatnich krawedzi!
                            foreach (var edge in hull) {
                                if (firstNew.points.Contains(edge.start) && firstNew.points.Contains(edge.end)) {
                                    edge.baseTriangle = firstNew;
                                } else if (secondNew.points.Contains(edge.start) && secondNew.points.Contains(edge.end)) {
                                    edge.baseTriangle = secondNew;
                                }
                            }
                            foreach (var neighbor in firstNew.triangleNeighbours) {
                                if (neighbor == secondNew || neighbor == null) {
                                    continue;
                                } else {
                                    legalisationQueue.Enqueue(neighbor);
                                }
                            }
                            foreach (var neighbor in secondNew.triangleNeighbours) {
                                if (neighbor == firstNew || neighbor == null) {
                                    continue;
                                } else {
                                    legalisationQueue.Enqueue(neighbor);
                                }
                            }
                        }

                    }
                }
            }
        }

        private List<Triangle> SwapEdgesAndFixNeighborRef(Triangle swappedTriangle, Triangle adjacentTriangle, DeluanVertex swappedVertex, DeluanVertex adjacentVertex)
        {
            var newTriangles = new List<Triangle>(2);

            var commonVertices = swappedTriangle.points.Where((p) => p != swappedVertex).ToList();
            var firstTriangleVertices = new List<DeluanVertex> {
                swappedVertex,
                adjacentVertex,
                commonVertices[0]
            };

            var secondTriangleVertices = new List<DeluanVertex> {
                swappedVertex,
                adjacentVertex,
                commonVertices[1]
            };

            firstTriangleVertices.Sort((p1, p2) => p1.X.CompareTo(p2.X));
            secondTriangleVertices.Sort((p1, p2) => p1.X.CompareTo(p2.X));

            Triangle first = MakeTriangleFromPoints(firstTriangleVertices);
            Triangle second = MakeTriangleFromPoints(secondTriangleVertices);

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

        private Triangle MakeTriangleFromPoints(List<DeluanVertex> Points)
        {
            var p0 = Points[0];
            var p1 = Points[1];
            var p2 = Points[2];
            var edge0 = new Edge(p0, p1);

            Triangle firstTriangle;

            if (!IsOnRight(edge0, p2)) {
                firstTriangle = new Triangle(p0, p1, p2);

            } else {
                firstTriangle = new Triangle(p1, p0, p2);
            }

            return firstTriangle;
        }

        private void FixNeighbors(Triangle beforeSwap1, Triangle beforeSwap2, Triangle afterSwap1, Triangle afterSwap2)
        {
            foreach (var neighbor in beforeSwap1.triangleNeighbours) {
                if (neighbor == beforeSwap2) {
                    continue;
                }
                if (CheckIfNeighbors(neighbor, afterSwap1)) {
                    neighbor.AddNeighbour(afterSwap1);
                    afterSwap1.AddNeighbour(neighbor);
                }
                if (CheckIfNeighbors(neighbor, afterSwap2)) {
                    neighbor.AddNeighbour(afterSwap2);
                    afterSwap2.AddNeighbour(neighbor);
                }
            }

        }

        private int GetNeighbourIndexByVertex(DeluanVertex vertex, DeluanVertex[] vertices)
        {
            var index = Array.IndexOf(vertices, vertex);
            return index == 2 ? 0 : index + 1;
        }

        private bool CheckIfNeighbors(Triangle t1, Triangle t2)
        {
            if (t1 == null || t2 == null) {
                return false;
            } else
                return t1.points.Intersect(t2.points).Count() == 2;
        }

        private void InitPoints(List<DeluanVertex> p)
        {
            int index = 0;
            foreach (var point in p) {
                point.Id = "p" + index++;
            }
        }

        private bool AreReversedEdges(Edge e1, Edge e2)
        {
            return (e1.start == e2.end && e1.end == e2.start);
        }

        private bool IsOnRight(Edge edge, DeluanVertex checkedPoint)
        {
            return (checkedPoint.X - edge.start.X) * (edge.end.Y - edge.start.Y) - (edge.end.X - edge.start.X) * (checkedPoint.Y - edge.start.Y) < 0;
        }

        private void InitializeHull(List<DeluanVertex> Points, LinkedList<Edge> Hull, List<Triangle> Triangles)
        {

            Triangle firstTriangle = MakeTriangleFromPoints(Points);

            var edge0 = new Edge(firstTriangle.points[0], firstTriangle.points[1]);
            var edge1 = new Edge(firstTriangle.points[1], firstTriangle.points[2]);
            var edge2 = new Edge(firstTriangle.points[2], firstTriangle.points[0]);

            edge0.baseTriangle = firstTriangle;
            edge1.baseTriangle = firstTriangle;
            edge2.baseTriangle = firstTriangle;

            Triangles.Add(firstTriangle);
            Hull.AddLast(edge0);
            Hull.AddLast(edge1);
            Hull.AddLast(edge2);

        }
    }
}
