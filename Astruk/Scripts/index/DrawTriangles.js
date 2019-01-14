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

function DrawTriangles(triangles) {
    console.log("drawTriangles triangles: " + triangles.length);
    let canvas = document.getElementById("imageDiv").childNodes[0];
    var g = canvas.getContext("2d");
    let i = 0;
    for (; i < triangles.length; i++) {
        DrawTriangle(triangles[i], g);
    }
}

function DrawTriangle(triangle, g) {
    let i = 0;
    g.strokeStyle = "black";
    //g.font = "17px Arial";
    g.beginPath();
    for (; i < 3; i++) {
        g.arc(triangle.points[i].X, triangle.points[i].Y, 4, 0, 2 * Math.PI);
        //g.fillText(triangle.points[i].Id, triangle.points[i].X, triangle.points[i].Y);
        g.moveTo(triangle.points[i].X, triangle.points[i].Y);
        if (i == 2) {
            g.lineTo(triangle.points[0].X, triangle.points[0].Y);
        } else {
            g.lineTo(triangle.points[i + 1].X, triangle.points[i + 1].Y);
        }
        g.stroke();
    }
}