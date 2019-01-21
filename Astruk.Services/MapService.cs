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
            SetBordersClockwise(ref Vertices);
            Triangulator TriangulationMaker = new Triangulator();
            List<DeluanVertex> Points = new List<DeluanVertex>();
            int i = 0;
            foreach (var keyObject in KeyObjects) {
                DeluanVertex v = new DeluanVertex(keyObject.X, keyObject.Y) {
                    Id = "p" + i++
                };
                Points.Add(v);
            }

            TestMap data = TriangulationMaker.Triangulate(Points);

            data.Vertices = Vertices;
            CoordsConstraints MapConstraints = new CoordsConstraints(Vertices);

            InitVoronoiCreation(data.triangles);
            GetVoronoiVertices(Points, Vertices);

            foreach (var t in data.triangles) {
                t.triangleNeighbours = null;
            }
            foreach (var p in data.points) {
                p.adjacentTriangles = null;
                p.exoTriangles = null;
            }

            return data;

        }

        private void SetBordersClockwise(ref IList<Vertex> Vertices)
        {
            double det = 0;
            for (int i = 0; i < Vertices.Count - 1; i++) {
                Vertex v1 = Vertices[i];
                Vertex v2 = Vertices[i + 1];
                det += (v2.X - v1.X) * (v2.Y + v1.Y);
            }

            if (det > 0) {
                List<Vertex> newList = new List<Vertex>();
                for (int i = 0; i < Vertices.Count; i++) {
                    newList.Add(Vertices[Vertices.Count - i - 1]);
                    newList[i].Id = i;
                }
                Vertices = newList;
            }
        }



        private void InitVoronoiCreation(List<Triangle> allTriangles)
        {
            foreach (var triangle in allTriangles) {
                for (int i = 0; i < 3; i++) {
                    DeluanVertex vertex = triangle.points[i];
                    var prevIndex = i == 0 ? 2 : i - 1;
                    if (triangle.triangleNeighbours[i] == null || triangle.triangleNeighbours[prevIndex] == null) {
                        vertex.isExo = true;
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
                //var visitedList = new List<Triangle>();

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
                        //visitedList.Add(currentTriangle);

                        for (int i = 0; i < 3; i++) {
                            var nextTriangle = currentTriangle.triangleNeighbours[i];
                            if (nextTriangle != null/* && !visitedList.Contains(nextTriangle) && deluanVertex.adjacentTriangles.Contains(nextTriangle)*/ && i == Array.IndexOf(currentTriangle.points, deluanVertex)) {
                                currentTriangle = nextTriangle;
                                break;
                            }
                        }
                    }
                    //popraw jak circum za konturem
                    /*
                    if (shouldCheckEdges) {
                        CheckEdges(deluanVertex, vertices);
                    }
                    */
                    if (shouldCheckEdges) {
                        CheckEdges(deluanVertex, vertices);
                    }


                } else if (deluanVertex.exoTriangles.Count == 1) {

                } else if (deluanVertex.exoTriangles.Count == 2) {

                    int edge1 = -1
                        , edge2 = -1,
                        vector1 = -1,
                        vector2 = -1;

                    Vector intersection = null;
                    int firstIntersectedEdgeId = -1;
                    int secondIntersectedEdgeId = -1;
                    Triangle exoTriangle = deluanVertex.exoTriangles[0];
                    Triangle endTriangle = deluanVertex.exoTriangles[1];
                    bool isClockwise = false;

                    Vector firstCircum = null
                        , secondCircum = null;


                    for (int i = 0; i < 3; i++) {
                        Triangle neighbor = exoTriangle.triangleNeighbours[i];
                        if (neighbor != null && Array.IndexOf(exoTriangle.points, deluanVertex) == i) {
                            isClockwise = true;
                            break;
                        }
                    }

                    if (!isClockwise) {
                        exoTriangle = deluanVertex.exoTriangles[1];
                        endTriangle = deluanVertex.exoTriangles[0];
                    }


                    //deluanVertex.voronoiVertices.Add(exoTriangle.circumcenter);
                    Triangle nextTriangle,
                        currentTriangle = exoTriangle;
                    //sprawdz, czy pierwsza krawedz nie przecina granic


                    int nextTriangleIndex = Array.IndexOf(exoTriangle.points, deluanVertex);
                    int nullNeighbourIndex = nextTriangleIndex - 1;
                    nullNeighbourIndex = nullNeighbourIndex < 0 ? 2 : nullNeighbourIndex;

                    nextTriangle = exoTriangle.triangleNeighbours[nextTriangleIndex];


                    if (MakeExoEdge(exoTriangle, vertices, nullNeighbourIndex, ref intersection, ref firstIntersectedEdgeId)) {
                        deluanVertex.voronoiVertices.Add(intersection);
                        deluanVertex.voronoiVertices.Add(exoTriangle.circumcenter);
                        firstCircum = exoTriangle.circumcenter;



                    } else {
                        //circum za granica
                        for (int i = 0; i < vertices.Count; i++) {
                            var startBorderVertex = vertices[i];
                            var endBorderVertex = i == vertices.Count - 1 ? vertices[0] : vertices[i + 1];
                            if (CheckIfLineSegmentsIntersects(exoTriangle.circumcenter, nextTriangle.circumcenter, startBorderVertex, endBorderVertex, ref intersection)) {
                                deluanVertex.voronoiVertices.Add(intersection);
                                firstIntersectedEdgeId = i;
                                firstCircum = nextTriangle.circumcenter;
                                break;
                            }
                        }
                    }
                    //currentTriangle = nextTriangle;


                    //teraz moge dodac nastepne trojkaty
                    //for (int j = 0; j < deluanVertex.adjacentTriangles.Count - 2; j++) {
                    while (currentTriangle != endTriangle) {
                        //lecimy po wszystkich trojkatach ktore maja ten wierzcholek

                        //sprawdz czy circum w srodku trojkata
                        if (currentTriangle.isObtuse) {
                            //poza trojkatem, sprawdz, czy w granicach
                            shouldCheckEdges = true;
                        }
                        //visitedList.Add(currentTriangle);

                        for (int i = 0; i < 3; i++) {
                            nextTriangle = currentTriangle.triangleNeighbours[i];
                            if (nextTriangle != null && i == Array.IndexOf(currentTriangle.points, deluanVertex)) {
                                currentTriangle = nextTriangle;
                                break;
                            }
                        }
                        deluanVertex.voronoiVertices.Add(currentTriangle.circumcenter);
                    }

                    int nullTriangleIndex = Array.IndexOf(currentTriangle.points, deluanVertex);
                    int previousTriangleIndex = nullTriangleIndex == 0 ? 2 : nullTriangleIndex - 1;
                    Triangle previousTriangle = currentTriangle.triangleNeighbours[previousTriangleIndex];
                    intersection = null;
                    if (MakeExoEdge(currentTriangle, vertices, nullTriangleIndex, ref intersection, ref secondIntersectedEdgeId)) {
                        deluanVertex.voronoiVertices.Add(intersection);
                        secondCircum = currentTriangle.circumcenter;
                    } else {
                        for (int i = 0; i < vertices.Count; i++) {
                            var startBorderVertex = vertices[i];
                            var endBorderVertex = i == vertices.Count - 1 ? vertices[0] : vertices[i + 1];
                            if (CheckIfLineSegmentsIntersects(currentTriangle.circumcenter, previousTriangle.circumcenter, startBorderVertex, endBorderVertex, ref intersection)) {
                                deluanVertex.voronoiVertices.Add(intersection);
                                secondIntersectedEdgeId = i;
                                secondCircum = previousTriangle.circumcenter;
                                break;
                            }
                        }
                    }
                    vector1 = deluanVertex.voronoiVertices.IndexOf(firstCircum);
                    vector2 = deluanVertex.voronoiVertices.IndexOf(secondCircum);

                    FixBorders(deluanVertex, vertices, deluanVertex.voronoiVertices[0], deluanVertex.voronoiVertices[deluanVertex.voronoiVertices.Count - 1], firstIntersectedEdgeId,
                        secondIntersectedEdgeId, vector1, vector2);


                    /*
                    if (shouldCheckEdges) {
                        CheckEdges(deluanVertex, vertices);
                    }
                    */
                    //sa dwa trojkaty
                    /*
                    foreach (var triangle in deluanVertex.exoTriangles) {
                        if (triangle.isObtuse) {
                            shouldCheckEdges = true;
                        }
                        MakeExoEdges(triangle, vertices);
                    }
                    */

                }


            }
        }


        private bool MakeExoEdge(Triangle triangle, IList<Vertex> borderVertices, int nullNeighborIndex, ref Vector intersection, ref int edgeId)
        {
            Edge exoTriangleEdge;
            Vector normalPoint;
            Vector normalLineAndBorderIntersection = null;
            CoordsConstraints constraints;

            switch (nullNeighborIndex) {
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
            normalPoint = new Vector((exoTriangleEdge.start.X + exoTriangleEdge.end.X) / 2, (exoTriangleEdge.start.Y + exoTriangleEdge.end.Y) / 2);
            constraints = MakeLineConstraints(triangle, normalPoint, nullNeighborIndex);

            for (int j = 0; j < borderVertices.Count; j++) {
                Vertex startBorderVertex = borderVertices[j];
                Vertex endBorderVertex = j == borderVertices.Count - 1 ? borderVertices[0] : borderVertices[j + 1];
                try {
                    normalLineAndBorderIntersection = GetIntersectionPoint(triangle.circumcenter, normalPoint, startBorderVertex, endBorderVertex);
                } catch (ArgumentException) {
                    continue;
                }
                if (IsPointInDomain(normalLineAndBorderIntersection, startBorderVertex, endBorderVertex, constraints)) {
                    intersection = normalLineAndBorderIntersection;
                    edgeId = j;
                    return true;
                }
            }
            return false;
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

                            //zoptymalizuj!!
                            for (int k = 0; k < vertices.Count; k++) {
                                Vertex w1 = vertices[k];
                                Vertex w2 = k == vertices.Count - 1 ? vertices[0] : vertices[k + 1];
                                if (CheckIfLineSegmentsIntersects(currentVertex, point, w1, w2)) {
                                    vertex1 = vertex1 - 1 < 0 ? point.voronoiVertices.Count - 1 : vertex1 - 1;
                                    break;
                                }
                            }
                            break;

                        }
                    } else {
                        if (CheckIfLineSegmentsIntersects(currentVertex, nextVertex, v1, v2, ref intersection2)) {
                            intersectionsFound++;
                            edge2 = j;
                            vertex2 = i;
                            for (int k = 0; k < vertices.Count; k++) {
                                Vertex w1 = vertices[k];
                                Vertex w2 = k == vertices.Count - 1 ? vertices[0] : vertices[k + 1];
                                if (CheckIfLineSegmentsIntersects(currentVertex, point, w1, w2)) {
                                    vertex2 = vertex2 + 1 == point.voronoiVertices.Count ? 0 : vertex2 + 1;
                                    break;
                                }
                            }
                            break;


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

        private bool FixBorders(DeluanVertex point, IList<Vertex> vertices, Vector intersection1, Vector intersection2, int edgeId1, int edgeId2, int vectorId1, int vectorId2)
        {
            var newList = new List<Vector>();
            Vector i1 = intersection1;
            Vector i2 = intersection2;

            Vector firstCircumInside, nextCircumInside, lastCircumInside;

            int edge1 = edgeId1,
            edge2 = edgeId2,
            v1 = vectorId1,
            v2 = vectorId2;

            if (edgeId1 > edgeId2) {
                i1 = intersection2;
                i2 = intersection1;
                edge1 = edgeId2;
                edge2 = edgeId1;
                v1 = vectorId2;
                v2 = vectorId1;
            }

            firstCircumInside = point.voronoiVertices[v1];
            lastCircumInside = point.voronoiVertices[v2];

            int prevIndex = v1 - 1 < 0 ? point.voronoiVertices.Count - 1 : v1 - 1;
            nextCircumInside = point.voronoiVertices[prevIndex];

            if (CheckIfLineSegmentsIntersects(point, vertices[edge1], /*point.voronoiVertices[v1]*/firstCircumInside, i1) || CheckIfLineSegmentsIntersects(point, vertices[edge1],
                nextCircumInside/*point.voronoiVertices[prevIndex]*/, i1)) {
                //ida rosnoca
                newList.Add(i1);
                for (int i = edge1 + 1; i <= edge2; i++) {
                    newList.Add(vertices[i]);
                }
                newList.Add(i2);
                Vector currentVertex = lastCircumInside;
                int nextIndex = v2;
                newList.Add(currentVertex);
                while (currentVertex != /*point.voronoiVertices[v1]*/firstCircumInside) {
                    nextIndex = nextIndex == point.voronoiVertices.Count - 1 ? 0 : nextIndex + 1;
                    currentVertex = point.voronoiVertices[nextIndex];
                    newList.Add(currentVertex);
                }
                point.voronoiVertices = newList;
                return true;
            } else {
                //idziem do przodu
                newList.Add(i2);
                int nextIndex = edge2;
                do {
                    nextIndex = nextIndex + 1 == vertices.Count ? 0 : nextIndex + 1;
                    newList.Add(vertices[nextIndex]);
                } while (nextIndex != edge1);
                newList.Add(i1);
                Vector currentVertex = point.voronoiVertices[v1];
                nextIndex = v1;
                newList.Add(currentVertex);
                while (currentVertex != lastCircumInside) {
                    nextIndex = nextIndex + 1 >= point.voronoiVertices.Count - 1 ? 0 : nextIndex + 1;
                    currentVertex = point.voronoiVertices[nextIndex];
                    newList.Add(currentVertex);
                }
                point.voronoiVertices = newList;
                return true;
            }

        }



        private void MakeExoEdges(Triangle triangle, IList<Vertex> borderVertices)
        {
            Vector normalPoint;
            Vector normalLineAndBorderIntersection = null;
            CoordsConstraints constraints;

            for (int i = 0; i < 3; i++) {
                if (triangle.triangleNeighbours[i] == null) {
                    Edge exoTriangleEdge = null;
                    switch (i) {
                        case 0:
                            exoTriangleEdge = new Edge(triangle.points[0], triangle.points[1]);
                            break;
                        case 1:
                            exoTriangleEdge = new Edge(triangle.points[1], triangle.points[2]);
                            break;
                        case 2:
                            exoTriangleEdge = new Edge(triangle.points[2], triangle.points[0]);
                            break;
                    }

                    normalPoint = new Vector((exoTriangleEdge.start.X + exoTriangleEdge.end.X) / 2, (exoTriangleEdge.start.Y + exoTriangleEdge.end.Y) / 2);
                    constraints = MakeLineConstraints(triangle, normalPoint, i);

                    for (int j = 0; j < borderVertices.Count; j++) {
                        Vertex startBorderVertex = borderVertices[j];
                        Vertex endBorderVertex = j == borderVertices.Count - 1 ? borderVertices[0] : borderVertices[j + 1];
                        try {
                            normalLineAndBorderIntersection = GetIntersectionPoint(triangle.circumcenter, normalPoint, startBorderVertex, endBorderVertex);
                        } catch (ArgumentException) {
                            continue;
                        }
                        if (IsPointInDomain(normalLineAndBorderIntersection, startBorderVertex, endBorderVertex, constraints)) {
                            triangle.borderIntersections[i] = new BorderIntersectionEdge(triangle.circumcenter, normalLineAndBorderIntersection, startBorderVertex);
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

        private bool IsPointInDomain(Vector intersectionPoint, Vector startBorderVertex, Vector endBorderVertex, CoordsConstraints c)
        {
            bool firstLower = startBorderVertex.X < endBorderVertex.X;
            var XMin = firstLower ? startBorderVertex.X : endBorderVertex.X;
            var XMax = firstLower ? endBorderVertex.X : startBorderVertex.X;

            firstLower = startBorderVertex.Y < endBorderVertex.Y;
            var YMin = firstLower ? startBorderVertex.Y : endBorderVertex.Y;
            var YMax = firstLower ? endBorderVertex.Y : startBorderVertex.Y;


            bool isInDomain = intersectionPoint.X >= c.XMin
                && intersectionPoint.X >= XMin
                && intersectionPoint.X <= c.XMax
                && intersectionPoint.X <= XMax
                && intersectionPoint.Y >= c.YMin
                && intersectionPoint.Y >= YMin
                && intersectionPoint.Y <= c.YMax
                && intersectionPoint.Y <= YMax;
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








    }
}
