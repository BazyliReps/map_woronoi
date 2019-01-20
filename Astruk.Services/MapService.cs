using System;
using System.Collections.Generic;
using System.Linq;
using Astruk.Common.Interfaces;
using Astruk.Common.Models;
using Astruk.Services.Helpers;

namespace Astruk.Services
{
    internal class MapService : IMapService
    {
        public TestMap GenerateMap(IList<Vertex> Vertices, IEnumerable<KeyMapObject> KeyObjects, IEnumerable<MapObjectType> Types, IEnumerable<MapObject> Objects)
        {
            Triangulator TriangulationMaker = new Triangulator();
            List<DeluanVertex> Points = new List<DeluanVertex>();
            int i = 0;
            foreach (var keyObject in KeyObjects) {
                DeluanVertex v = new DeluanVertex(keyObject.X, keyObject.Y) {
                    Id = "p" + i++
                };
                Points.Add(v);
            }

            TestMap data = TriangulationMaker.Triangulate(Points, Vertices);

            CoordsConstraints MapConstraints = new CoordsConstraints(Vertices);
            //var BorderEdges = MakeBorderEdges(Vertices);

            InitVoronoiCreation(data.triangles);
            GetVoronoiVertices(Points, Vertices);


            foreach (var p in Points) {
                foreach (var t in p.exoTriangles) {
                    if (t != null) {
                        if (t.borderIntersections.Count() == 0) {
                            throw new Exception("hwdp");
                        }
                    }
                }
            }

            foreach (var t in data.triangles) {
                t.triangleNeighbours = null;
            }
            foreach (var p in data.points) {
                p.adjacentTriangles = null;
                p.exoTriangles = null;
            }

            return data;

        }

        private void InitVoronoiCreation(List<Triangle> allTriangles)
        {
            foreach (var triangle in allTriangles) {
                for (int i = 0; i < 3; i++) {
                    DeluanVertex vertex = triangle.points[i];
                    var prevIndex = i == 0 ? 2 : i - 1;
                    if (triangle.triangleNeighbours[i] == null || triangle.triangleNeighbours[prevIndex] == null) {
                        vertex.exoTriangles.Add(triangle);
                    }
                    //tu sie jebac moze
                    vertex.adjacentTriangles.Add(triangle);

                }
            }
        }


        private void GetVoronoiVertices(List<DeluanVertex> allPoints, IList<Vertex> vertices)
        {
            foreach (var deluanVertex in allPoints) {

                var shouldCheckEdges = false;
                var visitedList = new List<Triangle>();

                //lecim po kazdym wierzcholku z listy
                if (deluanVertex.exoTriangles.Count == 0) {
                    // dla wierzcholkow wewnetrznych
                    var firstTriangle = deluanVertex.adjacentTriangles.First();
                    var currentTriangle = firstTriangle;
                    var numberOfTriangles = deluanVertex.adjacentTriangles.Count;


                    for (int j = 0; j < numberOfTriangles; j++) {
                        //lecimy po wszystkich trojkatach ktore maja ten wierzcholek

                        //sprawdz czy circum w srodku trojkata
                        if (currentTriangle.isObtuse) {
                            //poza trojkatem, sprawdz, czy w granicach
                            shouldCheckEdges = true;
                        }
                        deluanVertex.voronoiVertices.Add(currentTriangle.circumcenter);
                        visitedList.Add(currentTriangle);

                        for (int i = 0; i < 3; i++) {
                            var nextTriangle = currentTriangle.triangleNeighbours[i];
                            if (nextTriangle != null && !visitedList.Contains(nextTriangle) && deluanVertex.adjacentTriangles.Contains(nextTriangle) && i == Array.IndexOf(currentTriangle.points, deluanVertex)) {
                                currentTriangle = nextTriangle;
                                break;
                            }
                        }
                    }
                    //popraw ja kcircum za konturem
                    if (shouldCheckEdges) {
                        CheckEdges(deluanVertex, vertices);
                    }


                } else if (deluanVertex.exoTriangles.Count == 1) {

                } else if (deluanVertex.exoTriangles.Count == 2) {
                    //sa dwa trojkaty
                    foreach (var triangle in deluanVertex.exoTriangles) {
                        if (triangle.isObtuse) {
                            shouldCheckEdges = true;
                        }
                        MakeExoEdges(triangle, vertices);
                    }


                    var firstTriangle = deluanVertex.exoTriangles[0];
                    var secondTriangle = deluanVertex.exoTriangles[1];

                    //sprawdz dla ktorej zenetrznej krawedzi rysujemy
                    var vertexIndexFirstTriangle = Array.IndexOf(firstTriangle.points, deluanVertex);
                    var previousIndex = vertexIndexFirstTriangle == 0 ? 2 : vertexIndexFirstTriangle - 1;
                    vertexIndexFirstTriangle = firstTriangle.triangleNeighbours[vertexIndexFirstTriangle] == null ? vertexIndexFirstTriangle : previousIndex;

                    //var firstBorderVertex = firstTriangle.borderIntersections[vertexIndexFirstTriangle].StartVertexOfIntersectedEdge;


                    var vertexIndexSecondTriangle = Array.IndexOf(secondTriangle.points, deluanVertex);
                    previousIndex = vertexIndexFirstTriangle == 0 ? 2 : vertexIndexSecondTriangle - 1;
                    vertexIndexSecondTriangle = secondTriangle.triangleNeighbours[vertexIndexSecondTriangle] == null ? vertexIndexSecondTriangle : previousIndex;


                }

            }
        }

        private void CheckEdges(DeluanVertex point, IList<Vertex> vertices)
        {
            Vertex v1 = null;
            Vertex v2 = null;
            Vector currentVertex, nextVertex;
            Vector intersection1 = null;
            Vector intersection2 = null;
            int edge1 = 0, edge2 = 0;
            int vertex1 = 0, vertex2 = 0;

            int intersectionsFound = 0;

            for (int i = 0; i < point.voronoiVertices.Count && intersectionsFound < 2; i++) {
                currentVertex = point.voronoiVertices[i];
                var nextI = i == point.voronoiVertices.Count - 1 ? 0 : i + 1;
                nextVertex = point.voronoiVertices[nextI];

                for (int j = 0; j < vertices.Count && intersectionsFound < 2; j++) {
                    v1 = vertices[j];
                    var nextJ = j == vertices.Count - 1 ? 0 : j + 1;
                    v2 = vertices[nextJ];
                    if (intersectionsFound == 0) {
                        if (CheckIfLineSegmentsIntersects(currentVertex, nextVertex, v1, v2, ref intersection1)) {
                            intersectionsFound++;
                            edge1 = j;
                            vertex1 = i;
                        }
                    } else {
                        if (CheckIfLineSegmentsIntersects(currentVertex, nextVertex, v1, v2, ref intersection2)) {
                            intersectionsFound++;
                            edge2 = j;
                            vertex2 = i;
                        }
                    }
                }

            }


            if (intersectionsFound == 2) {

                var newVoronoiVertices = new List<Vector>();

                FixBorders(point, vertices, intersection1, intersection2, edge1, edge2, vertex1, vertex2);

                //test only
                point.adjacentTriangles[0].intersections.Add(new Vector(intersection1.X, intersection1.Y));
                point.adjacentTriangles[0].intersections.Add(new Vector(intersection2.X, intersection2.Y));



            }

        }

        private bool FixBorders(DeluanVertex point, IList<Vertex> vertices, Vector i1, Vector i2, int edge1, int edge2, int v1, int v2)
        {
            var newList = new List<Vector>();

            //edge1 < edge2
            if (edge1 < edge2) {

                if (CheckIfLineSegmentsIntersects(point, vertices[edge1], point.voronoiVertices[v1], i1)) {
                    //ida rosnoca
                    newList.Add(i1);
                    for (int i = edge1 + 1; i <= edge2; i++) {
                        newList.Add(vertices[i]);
                    }
                    newList.Add(i2);
                    Vector currentVertex = point.voronoiVertices[v2];
                    newList.Add(currentVertex);
                    int nextIndex = v2;
                    while (currentVertex != point.voronoiVertices[v1]) {
                        nextIndex = nextIndex == point.voronoiVertices.Count - 1 ? 0 : nextIndex + 1;
                        currentVertex = point.voronoiVertices[nextIndex];
                        newList.Add(currentVertex);
                    }
                    point.voronoiVertices = newList;
                    return true;
                } else {
                    //idziem do przodu
                    newList.Add(i2);
                    int nextIndex = edge2 + 1;
                    while (nextIndex <= edge1) {
                        nextIndex = nextIndex + 1 == vertices.Count ? 0 : nextIndex + 1;
                        newList.Add(vertices[nextIndex]);
                    }
                    newList.Add(i1);
                    Vector currentVertex = point.voronoiVertices[v1];
                    newList.Add(currentVertex);
                    nextIndex = v1 + 1;
                    while (currentVertex != point.voronoiVertices[v2]) {
                        nextIndex = nextIndex == point.voronoiVertices.Count - 1 ? 0 : nextIndex + 1;
                        currentVertex = point.voronoiVertices[nextIndex];
                        newList.Add(currentVertex);
                    }
                    point.voronoiVertices = newList;
                    return true;

                }



            } else {

            }

            return false;

        }


        private void MakeExoEdges(Triangle triangle, IList<Vertex> vertices)
        {
            Vector normalPoint;
            Vector intersectionPoint = null;
            CoordsConstraints constraints;

            for (int i = 0; i < 3; i++) {
                if (triangle.triangleNeighbours[i] == null) {
                    Edge exoEdge = null;
                    switch (i) {
                        case 0:
                            exoEdge = new Edge(triangle.points[0], triangle.points[1]);
                            break;
                        case 1:
                            exoEdge = new Edge(triangle.points[1], triangle.points[2]);
                            break;
                        case 2:
                            exoEdge = new Edge(triangle.points[2], triangle.points[0]);
                            break;
                    }

                    normalPoint = new Vector((exoEdge.start.X + exoEdge.end.X) / 2, (exoEdge.start.Y + exoEdge.end.Y) / 2);//GetNormalPointOnEdge(exoEdge.start, exoEdge.end, triangle.circumcenter);
                    constraints = MakeLineConstraints(triangle, normalPoint, i);

                    //foreach (var edge in vertices) {
                    for (int j = 0; j < vertices.Count; j++) {
                        Vertex start = vertices[j];
                        Vertex end = j == vertices.Count - 1 ? vertices[0] : vertices[j + 1];
                        try {
                            intersectionPoint = GetIntersectionPoint(triangle.circumcenter, normalPoint, start, end);
                        } catch (ArgumentException) {
                            continue;
                        }
                        if (IsPointInDomain(intersectionPoint, start, end, constraints)) {
                            triangle.borderIntersections[i] = new BorderIntersectionEdge(triangle.circumcenter, intersectionPoint, start);
                            continue;
                        }
                    }
                }
            }

        }

        private bool CheckIfLineSegmentsIntersects(Vector p, Vector pr, Vector q, Vector qs, ref Vector intersection)
        {
            double nullOffset = 0.000001;
            var r = pr - p;
            var s = qs - q;
            var product = r * s;
            if (Math.Abs(product) > nullOffset) {
                var t = ((q - p) * s) / product;
                var u = ((q - p) * r) / product;
                if (t >= 0 && t <= 1 && u >= 0 && u <= 1) {
                    intersection = p + t * r;
                    return true;
                }
            }
            return false;
        }

        private bool CheckIfLineSegmentsIntersects(Vector p, Vector pr, Vector q, Vector qs)
        {
            double nullOffset = 0.000001;
            var r = pr - p;
            var s = qs - q;
            var product = r * s;
            if (Math.Abs(product) > nullOffset) {
                var t = ((q - p) * s) / product;
                var u = ((q - p) * r) / product;
                if (t >= 0 && t <= 1 && u >= 0 && u <= 1) {
                    return true;
                }
            }
            return false;
        }

        private CoordsConstraints MakeLineConstraints(Triangle triangle, Vector normalPoint, int vertexIndex)
        {
            double XMin, XMax, YMin, YMax;

            int oppositeVertexId = vertexIndex == 0 ? 2 : vertexIndex - 1;
            double angle = triangle.GetAngleOnVertex(oppositeVertexId);
            bool circumcenterOutsideTriangle = angle > 90;


            if (circumcenterOutsideTriangle) {
                if (normalPoint.X > triangle.circumcenter.X) {
                    XMin = -double.MaxValue;
                    XMax = triangle.circumcenter.X;
                    if (normalPoint.Y > triangle.circumcenter.Y) {
                        YMin = -double.MaxValue;
                        YMax = triangle.circumcenter.Y;
                    } else {
                        YMin = triangle.circumcenter.Y;
                        YMax = double.MaxValue;
                    }

                } else {
                    XMin = triangle.circumcenter.X;
                    XMax = double.MaxValue;
                    if (normalPoint.Y > triangle.circumcenter.Y) {
                        YMin = -double.MaxValue;
                        YMax = triangle.circumcenter.Y;
                    } else {
                        YMin = triangle.circumcenter.Y;
                        YMax = double.MaxValue;
                    }
                }
            } else {

                if (normalPoint.X > triangle.circumcenter.X) {
                    XMin = triangle.circumcenter.X;
                    XMax = double.MaxValue;
                    if (normalPoint.Y > triangle.circumcenter.Y) {
                        YMin = triangle.circumcenter.Y;
                        YMax = double.MaxValue;
                    } else {
                        YMin = -double.MaxValue;
                        YMax = triangle.circumcenter.Y;
                    }
                } else {
                    XMin = -double.MaxValue;
                    XMax = triangle.circumcenter.X;
                    if (normalPoint.Y > triangle.circumcenter.Y) {
                        YMin = triangle.circumcenter.Y;
                        YMax = double.MaxValue;
                    } else {
                        YMin = -double.MaxValue;
                        YMax = triangle.circumcenter.Y;
                    }
                }
            }
            return new CoordsConstraints(XMin, XMax, YMin, YMax);
        }

        private bool IsPointInDomain(Vector p, Vector start, Vector end, CoordsConstraints c)
        {
            bool firstLower = start.X < end.X;
            var XMin = firstLower ? start.X : end.X;
            var XMax = firstLower ? end.X : start.X;

            firstLower = start.Y < end.Y;
            var YMin = firstLower ? start.Y : end.Y;
            var YMax = firstLower ? end.Y : start.Y;


            bool isInDomain = p.X >= XMin && p.X >= c.XMin && p.X <= XMax && p.X <= c.XMax && p.Y >= YMin && p.Y >= c.YMin && p.Y <= YMax && p.Y <= c.YMax;
            return isInDomain;
        }

        private Vector GetIntersectionPoint(Vector start1, Vector end1, Vector start2, Vector end2)
        {
            var nullOffset = 0.00001;

            var A1 = end1.Y - start1.Y;
            var B1 = start1.X - end1.X;
            var C1 = start1.X * A1 + start1.Y * B1;

            var A2 = end2.Y - start2.Y;
            var B2 = start2.X - end2.X;
            var C2 = start2.X * A2 + start2.Y * B2;

            var det = A1 * B2 - A2 * B1;

            if (Math.Abs(det) < nullOffset) {
                throw new ArgumentException("linie równoległe");
            }

            var x = (B2 * C1 - B1 * C2) / det;
            var y = (A1 * C2 - A2 * C1) / det;

            return new Vector(x, y);

        }

        private Vector GetNormalPointOnEdge(Vector start, Vector end, Vector point)
        {
            Vector edgeVector = new Vector(end.X - start.X, end.Y - start.Y);
            Vector pointVector = new Vector(point.X - start.X, point.Y - start.Y);

            double edgeVectorMagnitude = Magnitude(edgeVector);
            Vector normalizedEdgeVector = new Vector(edgeVector.X / edgeVectorMagnitude, edgeVector.Y / edgeVectorMagnitude);

            double dotProduct = edgeVector.X * pointVector.X + edgeVector.Y * pointVector.Y;

            return new Vector(start.X + normalizedEdgeVector.X * dotProduct, start.Y + normalizedEdgeVector.Y * dotProduct);

        }

        private double Magnitude(Vector p)
        {
            return Math.Sqrt(p.X * p.X + p.Y * p.Y);
        }







    }
}
