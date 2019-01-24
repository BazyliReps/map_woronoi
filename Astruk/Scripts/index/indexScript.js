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
    const xValue = document.getElementById("vertexX").value * 1;
    const yValue = document.getElementById("vertexY").value * 1;
    const hiddenData = document.getElementById("hiddenData");

    const allData = JSON.parse(hiddenData.textContent);
    clearData();

    const newId = allData.Vertices.length + 1;

    const newVertex = new Vertex(newId, xValue, yValue);
    allData.Vertices.push(newVertex);

    console.log(allData);

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

    sendAllData(allData);
    clearForms();
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