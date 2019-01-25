function clearForms() {
    const forms = document.getElementsByTagName("input");
    var i = 0;
    for (; i < forms.length; i++) {
        forms[i].value = "";
    }
}

function sendAllData(allData) {
    const jsonData = JSON.stringify(allData);
    const url = "Home/_LoadMap";

    const jsonDiv = document.getElementById("hiddenData");
    jsonDiv.insertAdjacentHTML("beforeend", jsonData);

    const keyObjects = document.getElementById("keyObjectsList");
    keyObjects.innerHTML = createKeyObjectsList(allData.KeyObjects);
    let i = 1;
    for (var vertex in allData.Vertices) {
        vertex.Id = i++;
    }

    const verticesList = document.getElementById("verticesList");
    verticesList.innerHTML = createVerticesList(allData.Vertices);

    const dataHtml = document.getElementById("hiddenData");
    dataHtml.innerHTML = JSON.stringify(allData);


    $("#map").load(url, allData, function(response, status) {
        if (status === "error") {
            $("#map").html(response);
        };
    });
}

function addVertex() {
    const xValue = document.getElementById("vertexX").value * 1;
    const yValue = document.getElementById("vertexY").value * 1;
    const hiddenData = document.getElementById("hiddenData");

    const allData = JSON.parse(hiddenData.textContent);
    clearData();

    const newId = allData.Vertices.length;

    const newVertex = new Vertex(xValue, yValue);
    newVertex.Id = newId;
    allData.Vertices.push(newVertex);


    createVerticesList(allData.Vertices);
    sendAllData(allData);
    clearForms();
}

function addKeyPoint() {

    const xValue = document.getElementById("keyX").value * 1;
    const yValue = document.getElementById("keyY").value * 1;
    const name = document.getElementById("keyName").value;
    const hiddenData = document.getElementById("hiddenData");

    const allData = JSON.parse(hiddenData.textContent);
    clearData();

    const newId = allData.KeyObjects.length + 1;
    const newKeyPoint = new KeyMapObject(newId, xValue, yValue, name);

    allData.KeyObjects.push(newKeyPoint);

    createKeyObjectsList(allData.Objects);
    sendAllData(allData);
    clearForms();
}

function deleteKeyObject(id) {
    const allData = JSON.parse(document.getElementById('hiddenData').textContent);
    const newKeyObjects = allData.KeyObjects.filter(function (keyObject) {
        return keyObject.Id != id;
    });

    allData.KeyObjects = newKeyObjects;
    sendAllData(allData);



}

function deleteVertex(id) {
    const allData = JSON.parse(document.getElementById('hiddenData').textContent);
    const newVertices = allData.Vertices.filter(function (vertex) {
        return vertex.Id != id;
    });
    allData.Vertices = newVertices;
    sendAllData(allData);
}

function createKeyObjectsList(objects) {
    let i = 0;
    let htmlString = "<ul style='overflow-y:scroll; height:300px'>";
    for (; i < objects.length; i++) {
        htmlString += `<div id="keyObject${objects[i].Id}">${objects[i].Name}: Id:${objects[i].Id}, x: ${objects[i].X}, y: ${objects[i].Y}`;
        htmlString += ` <button id="deleteKeyObjectButton${objects[i].Id}" class="keyObjectsDeleteButton" onclick="deleteKeyObject(${objects[i].Id})" style="margin: auto"">usuń</button></div>`
    }
    htmlString += "</ul>";
    return htmlString;
}

function createVerticesList(vertices) {
    let i = 0;
    let htmlString = "<ul style='overflow-y:scroll; height:300px'>";
    for (; i < vertices.length; i++) {
        htmlString += `<div id="vertex${i}">x: ${vertices[i].X}, y: ${vertices[i].Y}`;
        htmlString += ` <button id="deleteVertexButton" class="verticesDeleteButton" onclick="deleteVertex(${vertices[i].Id})"">usuń</button></div>`
    }
    htmlString += "</ul>";
    return htmlString;
}

function readFile(evt) {
    clearData();
    const input = evt.target;
    const reader = new FileReader();
    reader.onload = parseFile;

    reader.readAsText(input.files[0], "UTF-8");
}

function readImage(evt) {
    var image = new Image();

    image.src = URL.createObjectURL(evt.target.files[0]);
    if (typeof map !== "undefined") {
        var bg = map.image(image.src, map._viewBox[0], map._viewBox[1], "100%", "100%");
        bg.toBack();
    }
}

function getPositionId(object) {
    const allData = JSON.parse(document.getElementById("hiddenData").textContent);
    const mapObjectType = getType(allData, object.Type);
    var i = 0;
    for (; i < mapObjectType.Parameters.length; i++) {
        if (mapObjectType.Parameters[i].Name === "X") {
            return i;
        }
    }
}

function hide(id) {
    const elem = document.getElementById(id);
    if (elem.style.display === "block") {
        elem.style.display = "none";
    } else {
        elem.style.display = "block";
    }
}

function clearData() {
    const hid = document.getElementById("hiddenData");
    hid.innerHTML = "";
}