function parseFile(evt) {
    let content = evt.target.result;
    let out = document.getElementById("output");
    let lines = content.split('\n');
    let allData = new AllData();


    var hashCount = 0;
    for (i = 0; i < lines.length; i++) {

        if (lines[i].charAt(0) === '#') {
            hashCount++;
            continue;
        } else if (lines[i].charAt(0) === "\n") {
            continue;
        }

        switch (hashCount) {
            case 1:
                allData.addVertex(readVertex(lines[i]));
                break;
            case 2:
                allData.addKeyObject(readKeyMapObject(lines[i]));
                break;
            case 3:
                allData.addType(readMapObjectType(lines[i]));
                break;
            case 4:
                allData.addObject(readMapObject(lines[i]));
                break;
            default:
                break;
        }
    }
    //fillObjectsList(allData);
    sendAllData(allData);
    //showToggleObjectsViewButton();
    //drawData(allData, false);

} 

function cutDot(element) {
    const number = element.split('.');
    return number[0] * 1;

}

function readVertex(line) {
    const [id, x, y] = line.split(' ');
    let trimmedId = cutDot(id);
    if (isNumber(trimmedId)) {
        return new Vertex(trimmedId, x * 1, y * 1);
    }

}

function readKeyMapObject(line) {
    const [id, x, y, name] = line.split(' ');
    let trimmedId = cutDot(id);
    if (isNumber(trimmedId)) {
        return new KeyMapObject(id, x * 1, y * 1, name);
    }

}

function readMapObjectType(line) {
    const [id, typeName, ...others] = line.split(' ');
    var trimmedId = cutDot(id);
    if (isNumber(trimmedId)) {
        var objectType = new MapObjectType(trimmedId, typeName);
        let k = 0;
        for (k; k < others.length - 1; k += 2) {
                objectType.addPair(others[k], others[k + 1]);
            }
        return objectType;
    }
}

function readMapObject(line) {
    const [id, type, ...others] = line.split(' ');
    var trimmedId = cutDot(id);
    if (isNumber(trimmedId)) {
        var object = new MapObject(trimmedId, type);
        let j = 0;
            for (j; j < others.length; j++) {
                object.addParameter(others[j]);
            }
        return object;
    }
}

function isNumber(num) {
    if (typeof num === 'number' && num != ' ' && num != '\r' && num != '\n' && num != '') {
        return num - num === 0;
    }
    if (typeof num === 'string' && num.trim() !== '') {
        return true;
    }
    return false;
}
function getType(allData, typeName) {
    let i = 0;
    for (i; i < allData.Types.length; i++) {
        if (allData.Types[i].Name === typeName) {
            return allData.Types[i];
        }
    }
}