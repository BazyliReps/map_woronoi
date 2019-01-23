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
            List<DeluanVertex> DeluanVertices = new List<DeluanVertex>();
            int i = 0;
            foreach (var keyObject in KeyObjects) {
                DeluanVertex v = new DeluanVertex(keyObject.X, keyObject.Y) {
                    Id = "p" + i++
                };
                DeluanVertices.Add(v);
            }

            TestMap data = TriangulationMaker.Triangulate(DeluanVertices);
            data.Vertices = Vertices;
            CoordsConstraints MapConstraints = new CoordsConstraints(Vertices);

            InitVoronoiCreation(data.triangles);
            GetVoronoiVertices(DeluanVertices, Vertices, MapConstraints);

            foreach (var t in data.triangles) {
                t.triangleNeighbours = null;
            }
            foreach (var p in data.points) {
                p.adjacentTriangles = null;
                p.exoTriangles = null;
            }
            var pointsByEdges = data.points.OrderBy(p => p.voronoiVertices.Count);
            data.points = pointsByEdges.ToList();

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

        private void GetVoronoiVertices(List<DeluanVertex> allPoints, IList<Vertex> vertices, CoordsConstraints MapConstraints)
        {

            var borders = new List<Vector>() {
                    new Vector(MapConstraints.XMin, MapConstraints.YMin),
                    new Vector(MapConstraints.XMax, MapConstraints.YMin),
                    new Vector(MapConstraints.XMax, MapConstraints.YMax),
                    new Vector(MapConstraints.XMin, MapConstraints.YMax)
                };
            foreach (var deluanVertex in allPoints) {
                var shouldCheckEdges = false;
                if (deluanVertex.exoTriangles.Count == 0) {
                    var firstTriangle = deluanVertex.adjacentTriangles.First();
                    var currentTriangle = firstTriangle;
                    var numberOfTriangles = deluanVertex.adjacentTriangles.Count;

                    for (int j = 0; j < numberOfTriangles; j++) {
                        if (currentTriangle.isObtuse) {
                            shouldCheckEdges = true;
                        }
                        deluanVertex.voronoiVertices.Add(currentTriangle.circumcenter);
                        for (int i = 0; i < 3; i++) {
                            var nextTriangle = currentTriangle.triangleNeighbours[i];
                            if (nextTriangle != null && i == Array.IndexOf(currentTriangle.points, deluanVertex)) {
                                currentTriangle = nextTriangle;
                                break;
                            }
                        }
                    }
                    if (shouldCheckEdges) {
                        CheckEdges(deluanVertex, vertices, false);
                    }
                } else {
                    Vector intersection1 = null,
                        intersection2 = null;
                    int firstIntersectedEdgeId = -1;
                    int secondIntersectedEdgeId = -1;
                    Triangle exoTriangle = deluanVertex.exoTriangles.First();
                    Triangle endTriangle = deluanVertex.exoTriangles.Last();
                    bool isClockwise = false;
                    bool foundFirstExoLine = false;

                    if (deluanVertex.exoTriangles.Count == 1) {
                        secondIntersectedEdgeId = Array.IndexOf(exoTriangle.points, deluanVertex);
                        firstIntersectedEdgeId = secondIntersectedEdgeId == 0 ? 2 : secondIntersectedEdgeId - 1;
                        if (IsWithinBorders(exoTriangle.circumcenter, MapConstraints)) {
                            MakeExoEdge(exoTriangle, borders, firstIntersectedEdgeId, ref intersection1, ref firstIntersectedEdgeId, MapConstraints);
                            MakeExoEdge(exoTriangle, borders, secondIntersectedEdgeId, ref intersection2, ref secondIntersectedEdgeId, MapConstraints);
                            deluanVertex.voronoiVertices.Add(intersection1);
                            deluanVertex.voronoiVertices.Add(exoTriangle.circumcenter);
                            deluanVertex.voronoiVertices.Add(intersection2);
                        }
                    } else {
                        for (int i = 0; i < 3; i++) {
                            Triangle neighbor = exoTriangle.triangleNeighbours[i];
                            if (neighbor != null && Array.IndexOf(exoTriangle.points, deluanVertex) == i) {
                                isClockwise = true;
                                break;
                            }
                        }
                        if (!isClockwise) {
                            exoTriangle = deluanVertex.exoTriangles.Last();
                            endTriangle = deluanVertex.exoTriangles.First();
                        }
                        int nextTriangleIndex = Array.IndexOf(exoTriangle.points, deluanVertex);
                        int nullNeighborIndex = nextTriangleIndex - 1;
                        nullNeighborIndex = nullNeighborIndex < 0 ? 2 : nullNeighborIndex;

                        if (IsWithinBorders(exoTriangle.circumcenter, MapConstraints)) {
                            if (MakeExoEdge(exoTriangle, borders, nullNeighborIndex, ref intersection1, ref firstIntersectedEdgeId, MapConstraints)) {
                                deluanVertex.voronoiVertices.Add(intersection1);
                                foundFirstExoLine = true;
                            }
                        }
                        Triangle currentTriangle = exoTriangle;
                        Triangle nextTriangle = currentTriangle;
                        bool isOnRight;

                        for (int j = 0; j < deluanVertex.adjacentTriangles.Count; j++) {
                            for (int i = 0; i < 3; i++) {
                                nextTriangle = currentTriangle.triangleNeighbours[i];
                                if (nextTriangle != null && i == Array.IndexOf(currentTriangle.points, deluanVertex)) {
                                    break;
                                }
                            }
                            if (IsWithinBorders(currentTriangle.circumcenter, MapConstraints)) {
                                deluanVertex.voronoiVertices.Add(currentTriangle.circumcenter);
                            } else {
                                for (int i = 0; i < borders.Count; i++) {
                                    if (foundFirstExoLine) {
                                        if (CheckIfLineSegmentsIntersects(currentTriangle.circumcenter, deluanVertex.voronoiVertices.Last()
                                            , borders[i], borders[i == 3 ? 0 : i + 1],
                                        ref intersection2, out isOnRight)) {
                                            deluanVertex.voronoiVertices.Add(new Vector(intersection2));
                                            secondIntersectedEdgeId = i;
                                            break;
                                        }
                                    } else if(currentTriangle!=endTriangle) {
                                        if (CheckIfLineSegmentsIntersects(currentTriangle.circumcenter, nextTriangle.circumcenter, borders[i], borders[i == 3 ? 0 : i + 1],
                                        ref intersection1, out isOnRight)) {
                                            deluanVertex.voronoiVertices.Add(new Vector(intersection1));
                                            firstIntersectedEdgeId = i;
                                            foundFirstExoLine = true;
                                            break;
                                        }
                                    }
                                }
                            }
                            currentTriangle = nextTriangle;
                        }

                        nullNeighborIndex = Array.IndexOf(endTriangle.points, deluanVertex);
                        if (IsWithinBorders(endTriangle.circumcenter, MapConstraints)) {
                            if (MakeExoEdge(endTriangle, borders, nullNeighborIndex, ref intersection2, ref secondIntersectedEdgeId, MapConstraints)) {
                                deluanVertex.voronoiVertices.Add(new Vector(intersection2));
                            }
                        }
                    }
                    int currentId = secondIntersectedEdgeId;
                    if (firstIntersectedEdgeId != secondIntersectedEdgeId) {
                        do {
                            currentId = currentId == 3 ? 0 : currentId + 1;
                            deluanVertex.voronoiVertices.Add(borders[currentId]);

                        } while (currentId != firstIntersectedEdgeId);
                    }
                    CheckEdges(deluanVertex, vertices, true);
                }
            }
        }

        private bool IsWithinBorders(Vector v, CoordsConstraints constraints)
        {
            return (v.X > constraints.XMin && v.X < constraints.XMax && v.Y > constraints.YMin && v.Y < constraints.YMax);
        }


        private bool MakeExoEdge(Triangle triangle, List<Vector> borderVertices, int nullNeighborIndex, ref Vector intersection, ref int edgeId, CoordsConstraints mapConstraints)
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
                Vector startBorderVertex = borderVertices[j];
                Vector endBorderVertex = j == borderVertices.Count - 1 ? borderVertices[0] : borderVertices[j + 1];
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


        private void CheckEdges(DeluanVertex point, IList<Vertex> vertices, bool isOuterRegion)
        {
            Vector v1 = null;
            Vector v2 = null;
            Vector currentVoronoiVertex, nextVoronoiVertex;
            Vector intersection = null;
            int intersectionsFound = 0;
            bool outsideBorders = false;

            var toRemove = new List<Vector>();
            var foundIntersections = new List<Vector>();
            var innerVoronoiVertices = new List<Vector>();
            var idsOfEdges = new List<int>();
            var suspectedOfBeingOutside = new List<Vector>();

            for (int i = 0; i < point.voronoiVertices.Count; i++) {
                currentVoronoiVertex = point.voronoiVertices[i];
                var nextI = i == point.voronoiVertices.Count - 1 ? 0 : i + 1;
                nextVoronoiVertex = point.voronoiVertices[nextI];
                var timesFirstInside = 0;
                var timesSecondInside = 0;

                if (outsideBorders) {
                    toRemove.Add(currentVoronoiVertex);
                }

                for (int j = 0; j < vertices.Count; j++) {

                    v1 = vertices[j];
                    var nextJ = j == vertices.Count - 1 ? 0 : j + 1;
                    v2 = vertices[nextJ];

                    if (CheckIfLineSegmentsIntersects(currentVoronoiVertex, nextVoronoiVertex, v1, v2, ref intersection, out bool isFirstInside)) {
                        intersectionsFound++;
                        idsOfEdges.Add(j);
                        if (isFirstInside) {
                            if (foundIntersections.Count == 0) {
                                suspectedOfBeingOutside.Clear();
                            }
                            innerVoronoiVertices.Add(currentVoronoiVertex);
                            outsideBorders = true;
                            timesFirstInside++;
                        } else {
                            innerVoronoiVertices.Add(nextVoronoiVertex);
                            if (!toRemove.Contains(currentVoronoiVertex)) {
                                toRemove.Add(currentVoronoiVertex);
                            }
                            outsideBorders = false;
                            timesSecondInside++;
                        }
                        foundIntersections.Add(new Vector(intersection));
                    }
                }

                if (timesFirstInside > timesSecondInside) {
                    toRemove.Remove(currentVoronoiVertex);
                }

                if (foundIntersections.Count == 0) {
                    suspectedOfBeingOutside.Add(currentVoronoiVertex);
                }
            }
            if (foundIntersections.Count == 0) {
                suspectedOfBeingOutside.Clear();
            }
            toRemove.AddRange(suspectedOfBeingOutside);

            foreach (var v in toRemove) {
                point.voronoiVertices.Remove(v);
            }

            if (foundIntersections.Count > 0) {

                for (int i = 0; i < foundIntersections.Count - 1; i += 2) {
                    FixBorders(point, vertices, foundIntersections[i], foundIntersections[i + 1], idsOfEdges[i], idsOfEdges[i + 1], innerVoronoiVertices[i], innerVoronoiVertices[i + 1], isOuterRegion);
                }
            }
        }

        private bool FixBorders(DeluanVertex point, IList<Vertex> vertices, Vector intersection1, Vector intersection2, int edgeId1, int edgeId2, Vector firstInnerVoronoiVertex, Vector secondInnerVoronoiVertex, bool IsOuterRegion)
        {
            var newList = new List<Vector>();
            Vector i1 = intersection1;
            Vector i2 = intersection2;

            int firstInnerVoronoiVertexId = point.voronoiVertices.IndexOf(firstInnerVoronoiVertex);
            int secondInnerVoronoiVertexId = point.voronoiVertices.IndexOf(secondInnerVoronoiVertex);

            Vector firstCircumInside, nextCircumInside, lastCircumInside;

            int edge1 = edgeId1,
            edge2 = edgeId2,
            v1 = firstInnerVoronoiVertexId,
            v2 = secondInnerVoronoiVertexId;

            if (edgeId1 > edgeId2 || (edgeId1 == edgeId2 && (intersection1 - vertices[edgeId1]).Magnitude() > (intersection2 - vertices[edgeId1]).Magnitude())) {
                i1 = intersection2;
                i2 = intersection1;
                edge1 = edgeId2;
                edge2 = edgeId1;
                v1 = secondInnerVoronoiVertexId;
                v2 = firstInnerVoronoiVertexId;
            }
            try {

                firstCircumInside = point.voronoiVertices[v1];
                lastCircumInside = point.voronoiVertices[v2];
            } catch (ArgumentOutOfRangeException) {
                return false;
            }

            int prevIndex = v1 - 1 < 0 ? point.voronoiVertices.Count - 1 : v1 - 1;
            nextCircumInside = point.voronoiVertices[prevIndex];

            if (!CheckIfLineSegmentsIntersects(point, vertices[edge2], lastCircumInside, i2)) {
                newList.Add(i1);
                for (int i = edge1 + 1; i <= edge2; i++) {
                    newList.Add(vertices[i]);
                }
                newList.Add(i2);
                Vector currentVertex = lastCircumInside;
                int nextIndex = v2;
                newList.Add(currentVertex);
                while (currentVertex != firstCircumInside) {
                    nextIndex = nextIndex == point.voronoiVertices.Count - 1 ? 0 : nextIndex + 1;
                    currentVertex = point.voronoiVertices[nextIndex];
                    newList.Add(currentVertex);
                }
                point.voronoiVertices = newList;
                return true;
            } else {
                newList.Add(i2);
                int nextIndex = edge2;
                do {
                    nextIndex = nextIndex + 1 == vertices.Count ? 0 : nextIndex + 1;
                    newList.Add(vertices[nextIndex]);
                } while (nextIndex != edge1);
                newList.Add(i1);
                Vector currentVertex = firstCircumInside;
                nextIndex = v1;

                newList.Add(currentVertex);
                while (currentVertex != lastCircumInside) {
                    nextIndex = nextIndex == point.voronoiVertices.Count - 1 ? 0 : nextIndex + 1;
                    currentVertex = point.voronoiVertices[nextIndex];
                    newList.Add(currentVertex);
                }
                point.voronoiVertices = newList;
                return true;
            }

        }

        private bool CheckIfLineSegmentsIntersects(Vector start, Vector end, Vector edgeStart, Vector edgeEnd, ref Vector intersection, out bool firstOnRight)
        {
            double nullOffset = 0.00000001;
            var r = end - start;
            var s = edgeEnd - edgeStart;
            var product = r * s;
            if (Math.Abs(product) > nullOffset) {
                var t = ((edgeStart - start) * s) / product;
                var u = ((edgeStart - start) * r) / product;
                if (t >= 0 && t <= 1 && u >= 0 && u <= 1) {
                    intersection = start + t * r;
                    if (IsOnRight(edgeStart, edgeEnd, start)) {
                        firstOnRight = true;
                    } else {
                        firstOnRight = false;
                    }
                    return true;
                }
            }
            firstOnRight = false;
            return false;
        }

        private bool IsOnRight(Vector edgeVertex1, Vector edgeVertex2, Vector checkedPoint)
        {
            return (checkedPoint.X - edgeVertex1.X) * (edgeVertex2.Y - edgeVertex1.Y) - (edgeVertex2.X - edgeVertex1.X) * (checkedPoint.Y - edgeVertex1.Y) < 0;
        }

        private bool CheckIfLineSegmentsIntersects(Vector p, Vector pr, Vector q, Vector qs)
        {
            double nullOffset = 0.00000001;
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

            bool horizontalParallel = YMin == YMax;
            bool verticalParallel = XMin == XMax;


            bool isInDomain = intersectionPoint.X >= c.XMin
                && (intersectionPoint.X >= XMin || verticalParallel)
                && intersectionPoint.X <= c.XMax
                && (intersectionPoint.X <= XMax || verticalParallel)
                && intersectionPoint.Y >= c.YMin
                && (intersectionPoint.Y >= YMin || horizontalParallel)
                && intersectionPoint.Y <= c.YMax
                && (intersectionPoint.Y <= YMax || horizontalParallel);
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
