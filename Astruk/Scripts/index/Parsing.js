function parseFile(evt) {
    const content = evt.target.result;
    const out = document.getElementById("output");
    const lines = content.split("\n");
    const allData = new AllData();


    var hashCount = 0;
    for (i = 0; i < lines.length; i++) {

        if (lines[i].charAt(0) === "#") {
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
    sendAllData(allData);

}

function cutDot(element) {
    const number = element.split(".");
    return number[0] * 1;

}

function trimAll(string) {
    return string.replace(/(\r\n|\n|\r)/gm, "");
}

function readVertex(line) {
    const [id, x, y] = line.split(" ");
    const trimmedId = cutDot(id);
    if (isNumber(trimmedId)) {
        const v = new Vertex(x * 1, y * 1);
        v.Id = id - 1;
        return v;
    }

}

function readKeyMapObject(line) {
    const [id, x, y, name] = line.split(" ");
    const trimmedId = cutDot(id);
    if (isNumber(trimmedId)) {
        return new KeyMapObject(trimmedId, x * 1, y * 1, trimAll(name));
    }

}

function readMapObjectType(line) {
    const [id, typeName, ...others] = line.split(" ");
    const trimmedId = cutDot(id);
    if (isNumber(trimmedId)) {
        const objectType = new MapObjectType(trimmedId, trimAll(typeName));
        let k = 0;
        for (k; k < others.length - 1; k += 2) {
            objectType.addPair(trimAll(others[k]), trimAll(others[k + 1]));
        }
        return objectType;
    }
}

function readMapObject(line) {
    const [id, type, ...others] = line.split(" ");
    const trimmedId = cutDot(id);
    if (isNumber(trimmedId)) {
        const object = new MapObject(trimmedId, trimAll(type));
        let j = 0;
        for (j; j < others.length; j ++) {
            object.addParameter(trimAll(others[j]));
        }
        return object;
    }
}

function isNumber(num) {
    if (typeof num === "number" && num != " " && num != "\r" && num != "\n" && num != "") {
        return num - num === 0;
    }
    if (typeof num === "string" && num.trim() !== "") {
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