    



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

} 


function readFile(evt) {
    clearData();
    var input = evt.target;
    var reader = new FileReader();
    reader.onload = parseFile;

    reader.readAsText(input.files[0], "UTF-8");
} 


function sendAllData(allData) {
    let jsonData = JSON.stringify(allData);
    const http = new XMLHttpRequest();
    const url = "Home/LoadMap";

    let jsonDiv = document.getElementById("hiddenData");
    jsonDiv.insertAdjacentHTML('beforeend', jsonData);

    http.open('POST', url, true);
    http.setRequestHeader('Content-type', 'application/json');
    http.send(jsonData);

    

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




