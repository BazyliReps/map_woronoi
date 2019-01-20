class Triangle {
    constructor(p1, p2, p3) {
        this.points = [];
        this.points.push(p1);
        this.points.push(p2);
        this.points.push(p3);
    }
}

class Point {
    constructor(x, y) {
        this.x = x;
        this.y = y;
    }
}

function DrawTriangles(triangles, context) {
    console.log("drawTriangles triangles: " + triangles.length);
    let i = 0;
    for (; i < triangles.length; i++) {
        DrawTriangle(triangles[i], context);
    }
}

function DrawVertices(vertices, context) {
    context.strokeStyle = "red";
    context.beginPath();
    let i = 0;
    for (; i < vertices.length; i++) {
        context.fillText(vertices[i].Id, vertices[i].X, vertices[i].Y);
        context.moveTo(vertices[i].X, vertices[i].Y);
        if (i == vertices.length - 1) {
            context.lineTo(vertices[0].X, vertices[0].Y);
        } else {
            context.lineTo(vertices[i + 1].X, vertices[i + 1].Y);
        }
    }
    context.stroke();
}

function DrawVoronoi(vertices, context) {
    context.strokeStyle = "blue";
    context.beginPath();
    let i = 0;
    for (; i < vertices.length; i++) {
        context.fillText(vertices[i].Id, vertices[i].X, vertices[i].Y);
        context.moveTo(vertices[i].X, vertices[i].Y);
        if (i == vertices.length - 1) {
            context.lineTo(vertices[0].X, vertices[0].Y);
        } else {
            context.lineTo(vertices[i + 1].X, vertices[i + 1].Y);
        }
    }
    context.stroke();
}

function DrawKeyPoints(triangles, context) {
    let i = 0;
    for (; i < triangles.length; i++) {
        let j = 0;
        for (; j < triangles[i].points.length; j++) {
            context.beginPath();
            context.arc(triangles[i].points[j].X, triangles[i].points[j].Y, 4, 0, 2 * Math.PI);
            context.fill();
            context.closePath();
            //g.fillText(triangle.points[i].Id, triangle.points[i].X, triangle.points[i].Y);

        }
    }
}

function DrawTriangle(triangle, context) {
    let i = 0;
    context.strokeStyle = "black";
    context.font = "17px Arial";

    context.strokeStyle = "blue";
    /*
    g.beginPath();
    g.arc(triangle.circumcenter.X, triangle.circumcenter.Y, triangle.radius, 0, 2 * Math.PI);
    g.closePath();
    g.stroke();
    */
    context.beginPath();
    context.arc(triangle.circumcenter.X, triangle.circumcenter.Y, 4, 0, 2 * Math.PI);
    context.closePath();
    context.stroke();

    if (triangle.intersections.length > 0) {
        let j = 0;
        for (; j < triangle.intersections.length; j++) {

            context.strokeStyle = "green";
            context.beginPath();
            context.arc(triangle.intersections[j].X, triangle.intersections[j].Y, 4, 0, 2 * Math.PI);
            context.closePath();
            context.stroke();
        }
    }


    context.strokeStyle = "black";

    for (; i < 3; i++) {
        //g.arc(triangle.points[i].X, triangle.points[i].Y, 4, 0, 2 * Math.PI);
        context.fillText(triangle.points[i].Id, triangle.points[i].X, triangle.points[i].Y);


        if (triangle.borderIntersections[i] != null) {
            context.beginPath();
            //context.arc(triangle.borderIntersections[i].X, triangle.borderIntersections[i].Y, 4, 0, 2 * Math.PI);
            context.moveTo(triangle.borderIntersections[i].Start.X, triangle.borderIntersections[i].Start.Y);
            context.lineTo(triangle.borderIntersections[i].End.X, triangle.borderIntersections[i].End.Y);
            context.closePath();
            context.stroke();
        }

        context.beginPath();
        let nextX, nextY;
        context.moveTo(triangle.points[i].X, triangle.points[i].Y);

        nextX = i == 2 ? triangle.points[0].X : triangle.points[i + 1].X;
        nextY = i == 2 ? triangle.points[0].Y : triangle.points[i + 1].Y;

        context.lineTo(nextX, nextY);
        context.closePath();
        context.stroke();
        //context.fillText(i, (triangle.points[i].X + nextX) / 2, (triangle.points[i].Y + nextY) / 2);
    }

}