var keyPointXform = document.getElementById("keyX");
var keyPointYform = document.getElementById("keyY");
var keyPointNameForm = document.getElementById("keyName");

var vertexXform = document.getElementById("vertexX");
var vertexYform = document.getElementById("vertexY");

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

    $("#map").load(url, allData);
}

function addVertex() {
    const xValue = vertexXform.value * 1;
    const yValue = vertexYform.value * 1;
    const hiddenData = document.getElementById("hiddenData");

    const allData = JSON.parse(hiddenData.textContent);
    clearData();

    const newId = allData.Vertices.length + 1;

    const newVertex = new Vertex(newId, xValue, yValue);
    allData.Vertices.push(newVertex);

    console.log(allData);

    fillObjectsList(allData);
    sendAllData(allData);
    clearForms();
    drawData(allData, true);
}

function addKeyPoint() {

    const xValue = keyPointXform.value * 1;
    const yValue = keyPointYform.value * 1;
    const name = keyPointNameForm.value;
    const hiddenData = document.getElementById("hiddenData");

    const allData = JSON.parse(hiddenData.textContent);
    clearData();

    const newId = allData.KeyObjects.length + 1;
    const newKeyPoint = new KeyMapObject(newId, xValue, yValue, name);

    allData.KeyObjects.push(newKeyPoint);

    fillObjectsList(allData);
    sendAllData(allData);
    clearForms();
    drawData(allData, false);
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

function fillObjectsList(allData) {
    const objectsDisplay = document.getElementById("objects");
    const objectList = allData.Objects;
    var output = "<ul>";
    for (i = 0; i < objectList.length; i++) {
        const currentObject = objectList[i];
        const currentObjectType = getType(allData, currentObject.Type);
        const divId = currentObject.Id + currentObject.Type;

        output += makeObjectListElementOpening(divId);
        output += "Id: ";
        output += currentObject.Id + " ";

        output += "Typ: ";
        output += currentObject.Type + " ";
        let j = 0;
        output += makeParametersListOpening(divId);
        for (; j < currentObject.Parameters.length; j++) {
            output += "<li>" + currentObjectType.Parameters[j].Name + ": " + currentObject.Parameters[j] + "</li>";
        }
        output += "</ul>";

        output += "</li>";
    }
    objectsDisplay.insertAdjacentHTML("beforeend", output);
}

function makeObjectListElementOpening(divId) {
    return "<li id=\"" + divId + "\"  onclick=\"hide('" + divId + "parameters')\">";
}

function makeParametersListOpening(divId) {
    return "<ul id=\"" + divId + "parameters\" style=\"display:none\">";
}

function hide(id) {
    const elem = document.getElementById(id);
    if (elem.style.display === "block") {
        elem.style.display = "none";
    } else {
        elem.style.display = "block";
    }
}

function showToggleObjectsViewButton() {
    const button = document.getElementById("toggleObjectsViewButton");
    button.style.display = "block";
}

function clearData() {
    const hid = document.getElementById("hiddenData");
    hid.innerHTML = "";
}