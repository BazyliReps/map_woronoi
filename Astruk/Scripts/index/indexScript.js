var keyPointXform = document.getElementById("keyX");
var keyPointYform = document.getElementById("keyY");
var keyPointNameForm = document.getElementById("keyName");

var vertexXform = document.getElementById("vertexX");
var vertexYform = document.getElementById("vertexY");

function clearForms() {
    var forms = document.getElementsByTagName('input');
    var i = 0;
    for (; i < forms.length; i++) {
        forms[i].value = '';
    }
}

function getMousePos(canvas, evt) {
    var context = canvas.getContext("2d");
    var rect = canvas.getBoundingClientRect();
    return {
        x: evt.clientX - rect.left,
        y: evt.clientY - rect.top
    };
}


function draw(evt, canvas) {
    var pos = getMousePos(canvas, evt);

    document.getElementById("mouse").insertAdjacentHTML("X: " + pos.x + " Y: " + pos.y);
}

//POST
function sendAllData(allData) {
    var jsonData = JSON.stringify(allData);
    var url = "Home/_LoadMap";

    var jsonDiv = document.getElementById("hiddenData");
    jsonDiv.insertAdjacentHTML('beforeend', jsonData);

    $("#map").load(url, jsonData);
}


function addVertex() {

    var xValue = vertexXform.value * 1;
    var yValue = vertexYform.value * 1;
    var hiddenData = document.getElementById('hiddenData');

    var allData = JSON.parse(hiddenData.textContent);
    clearData();


    var newId = allData.Vertices.length + 1;

    var newVertex = new Vertex(newId, xValue, yValue);
    allData.Vertices.push(newVertex);

    console.log(allData);

    fillObjectsList(allData);
    sendAllData(allData);
    clearForms();
    drawData(allData, true);
}

function addKeyPoint() {

    var xValue = keyPointXform.value * 1;
    var yValue = keyPointYform.value * 1;
    var name = keyPointNameForm.value;
    var hiddenData = document.getElementById('hiddenData');


    var allData = JSON.parse(hiddenData.textContent);
    clearData();


    var newId = allData.KeyObjects.length + 1;
    var newKeyPoint = new KeyMapObject(newId, xValue, yValue, name);

    allData.KeyObjects.push(newKeyPoint);

    console.log(allData);

    fillObjectsList(allData);
    sendAllData(allData);
    clearForms();
    drawData(allData, false);
}


function readFile(evt) {
    clearData();
    var input = evt.target;
    var reader = new FileReader();
    reader.onload = parseFile;

    reader.readAsText(input.files[0], "UTF-8");
}

function readImage(evt) {
    var imageDiv = document.getElementById("imageDiv");
    var imgSrc = document.getElementById("imgSrc");
    var image = new Image();

    var hiddenData = document.getElementById('hiddenData');
    var allData = JSON.parse(hiddenData.textContent);

    image.src = URL.createObjectURL(evt.target.files[0]);
    imgSrc.innerHTML = image.src;
    image.onload = function() {
        var canvas = imageDiv.childNodes[0];
        var g = canvas.getContext("2d");
        g.fillStyle = "white";
        g.fillRect(0, 0, canvas.width, canvas.height);
        g.drawImage(image, 0, 0);
        drawData(allData, false);
    }
}

function getPositionId(object) {
    var allData = JSON.parse(document.getElementById("hiddenData").textContent);
    var mapObjectType = getType(allData, object.Type);
    var i = 0;
    for (; i < mapObjectType.Parameters.length; i++) {
        if (mapObjectType.Parameters[i].Name === "X") {
            return i;
        }
    }
}


function fillObjectsList(allData) {
    var objectsDisplay = document.getElementById("objects");
    var objectList = allData.Objects;
    var output = "<ul>";
    for (i = 0; i < objectList.length; i++) {
        var currentObject = objectList[i];
        var currentObjectType = getType(allData, currentObject.Type);
        var divId = currentObject.Id + currentObject.Type;

        output += makeObjectListElementOpening(divId);
        output += "Id: ";
        output += currentObject.Id + " ";

        output += "Typ: ";
        output += currentObject.Type + " ";
        var j = 0;
        output += makeParametersListOpening(divId);
        for (; j < currentObject.Parameters.length; j++) {
            output += "<li>" + currentObjectType.Parameters[j].Name + ": " + currentObject.Parameters[j] + "</li>";
        }
        output += "</ul>";

        output += "</li>"
    }
    objectsDisplay.insertAdjacentHTML('beforeend', output);
}

function makeObjectListElementOpening(divId) {
    return "<li id=\"" + divId + "\"  onclick=\"hide('" + divId + "parameters')\">";
}

function makeParametersListOpening(divId) {
    return "<ul id=\"" + divId + "parameters\" style=\"display:none\">";
}


function hide(id) {
    var elem = document.getElementById(id);
    if (elem.style.display == "block") {
        elem.style.display = "none";
    } else {
        elem.style.display = "block";
    }
}

function showToggleObjectsViewButton() {
    var button = document.getElementById("toggleObjectsViewButton");
    button.style.display = "block";
}

function clearData() {
    var obj = document.getElementById('objects');
    var hid = document.getElementById('hiddenData');
    obj.innerHTML = "";
    hid.innerHTML = "";
}