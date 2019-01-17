



var keyPointXform = document.getElementById("keyX");
var keyPointYform = document.getElementById("keyY");
var keyPointNameForm = document.getElementById("keyName");

var vertexXform = document.getElementById("vertexX");
var vertexYform = document.getElementById("vertexY");

function clearForms() {
    let forms = document.getElementsByTagName('input');
    let i = 0;
    for (; i < forms.length; i++) {
        forms[i].value = '';
    }
}

//POST
function sendAllData(allData) {
    let jsonData = JSON.stringify(allData);
    const url = "Home/LoadMap";

    console.log(allData);

    let jsonDiv = document.getElementById("hiddenData");
    jsonDiv.insertAdjacentHTML('beforeend', jsonData);



    $.ajax({
        url: url,
        type: "POST",
        data: jsonData,
        contentType: "application/json; charset = utf-8",
        dataType: "json",
        success: function (returnData) {
            let canvas = document.getElementById("imageDiv").childNodes[0];
            var context = canvas.getContext("2d");
            context.clearRect(0, 0, canvas.width, canvas.height);

            console.log(returnData);
            DrawTriangles(returnData.triangles, context);
            DrawVertices(allData.Vertices, context);
            let i = 0;
            for (; i < returnData.points.length; i++) {
                DrawVertices(returnData.points[i].voronoiVertices, context);
            }
            DrawKeyPoints(returnData.triangles, context);
        }
    })
}


function addVertex() {

    let xValue = vertexXform.value * 1;
    let yValue = vertexYform.value * 1;
    let hiddenData = document.getElementById('hiddenData');


    let allData = JSON.parse(hiddenData.textContent);
    clearData();


    let newId = allData.Vertices.length + 1;

    let newVertex = new Vertex(newId, xValue, yValue);
    allData.Vertices.push(newVertex);

    console.log(allData);

    fillObjectsList(allData);
    sendAllData(allData);
    clearForms();
    drawData(allData, true);

}

function addKeyPoint() {

    let xValue = keyPointXform.value * 1;
    let yValue = keyPointYform.value * 1;
    let name = keyPointNameForm.value;
    let hiddenData = document.getElementById('hiddenData');


    let allData = JSON.parse(hiddenData.textContent);
    clearData();



    let newId = allData.KeyObjects.length + 1;
    let newKeyPoint = new KeyMapObject(newId, xValue, yValue, name);

    allData.KeyObjects.push(newKeyPoint);

    console.log(allData);

    fillObjectsList(allData);
    sendAllData(allData);
    clearForms();
    drawData(allData, false);
}


function readFile(evt) {
    clearData();
    let input = evt.target;
    let reader = new FileReader();
    reader.onload = parseFile;

    reader.readAsText(input.files[0], "UTF-8");
}

function readImage(evt) {
    let imageDiv = document.getElementById("imageDiv");
    let imgSrc = document.getElementById("imgSrc");
    let image = new Image();

    let hiddenData = document.getElementById('hiddenData');
    let allData = JSON.parse(hiddenData.textContent);

    image.src = URL.createObjectURL(evt.target.files[0]);
    imgSrc.innerHTML = image.src;
    image.onload = function () {
        var canvas = imageDiv.childNodes[0];
        var g = canvas.getContext("2d");
        g.fillStyle = "white";
        g.fillRect(0, 0, canvas.width, canvas.height);
        g.drawImage(image, 0, 0);
        drawData(allData, false);
    }
}

function getPositionId(object) {
    let allData = JSON.parse(document.getElementById("hiddenData").textContent);
    let mapObjectType = getType(allData, object.Type);
    let i = 0;
    for (; i < mapObjectType.Parameters.length; i++) {
        if (mapObjectType.Parameters[i].Name === "X") {
            return i;
        }
    }
}


function fillObjectsList(allData) {
    var objectsDisplay = document.getElementById("objects");
    var objectList = allData.Objects;
    let output = "<ul>";
    for (i = 0; i < objectList.length; i++) {
        let currentObject = objectList[i];
        let currentObjectType = getType(allData, currentObject.Type);
        let divId = currentObject.Id + currentObject.Type;

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
    let elem = document.getElementById(id);
    if (elem.style.display == "block") {
        elem.style.display = "none";
    } else {
        elem.style.display = "block";
    }
}

function showToggleObjectsViewButton() {
    let button = document.getElementById("toggleObjectsViewButton");
    button.style.display = "block";
}

function clearData() {
    let obj = document.getElementById('objects');
    let hid = document.getElementById('hiddenData');
    obj.innerHTML = "";
    hid.innerHTML = "";
}

