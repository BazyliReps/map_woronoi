function drawData(allData, eraseAll) {

    let canvas = document.getElementById("imageDiv").childNodes[0];
    var g = canvas.getContext("2d");

    if (eraseAll) {
        let imgSrc = document.getElementById("imgSrc");
        let image = new Image();
        image.src = imgSrc.innerHTML;
        g.drawImage(image, 0, 0);
    }

    let vertices = allData.Vertices;
    drawBorders(vertices, g);
    drawKeyObjects(allData.KeyObjects, g);
    drawObjects(allData.Objects, g);

}

function drawBorders(vertices, g) {
    g.beginPath();
    let i = 0;
    for (; i < vertices.length; i++) {
        g.arc(vertices[i].X, vertices[i].Y, 2, 0, 2 * Math.PI);
        g.fill();
        g.moveTo(vertices[i].X, vertices[i].Y);
        if (i == vertices.length - 1) {
            g.lineTo(vertices[0].X, vertices[0].Y);
        } else {
            g.lineTo(vertices[i + 1].X, vertices[i + 1].Y);
        }
    }
    g.strokeStyle = "black";
    g.stroke();
}

function drawKeyObjects(keyObjects, g) {
    let i = 0;
    for (; i < keyObjects.length; i++) {
        g.beginPath();
        g.arc(keyObjects[i].X, keyObjects[i].Y, 10, 0, 2 * Math.PI);
        g.fillStyle = "black";
        g.fill();
        g.font = "20px Arial";
        g.fillText(keyObjects[i].Name, keyObjects[i].X + 20, keyObjects[i].Y);
    }
}

function drawObjects(objects, g) {
    let i = 0;
    for (; i < objects.length; i++) {

        let indexOfX = getPositionId(objects[i]);
        let x = objects[i].Parameters[indexOfX] * 1;
        let y = objects[i].Parameters[indexOfX + 1] * 1;
        

        g.beginPath();
        g.arc(x, y, 10, 0, 2 * Math.PI);
        g.fillStyle = "red";
        g.fill();

        g.fillStyle = "black";
        g.font = "20px Arial";
        //g.fillText(objects[i].Type, x + 20, y);
        
    }
}