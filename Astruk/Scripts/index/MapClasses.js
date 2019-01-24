class Vertex {
    constructor(x, y) {
        this.X = x;
        this.Y = y;
        this.Id = -1;
    }

}

class KeyMapObject {
    constructor(id, x, y, name) {
        this.Id = id;
        this.X = x;
        this.Y = y;
        this.Name = name;
    }

}

class Pair {
    constructor(key, value) {
        this.key = value;
    }

}

class MapObjectType {
    constructor(id, typeName) {
        this.Id = id;
        this.Name = typeName;
        this.Keys = [];
        this.Values = [];
    }

    addPair(fieldName, type) {
        this.Keys.push(fieldName);
        this.Values.push(type);
    }

}

class MapObject {
    constructor(id, type) {
        this.Id = id;
        this.Type = type;
        this.Parameters = [];
    }

    addParameter(param) {
        this.Parameters.push(param);
    }

}

class AllData {
    constructor() {
        this.Vertices = [];
        this.KeyObjects = [];
        this.Types = [];
        this.Objects = [];
    }

    addVertex(vertex) {
        if (vertex != undefined) {
            this.Vertices.push(vertex);
        }
    }

    addKeyObject(keyObject) {
        if (keyObject != undefined)
            this.KeyObjects.push(keyObject);
    }

    addType(type) {
        if (type != undefined)
            this.Types.push(type);
    }

    addObject(object) {
        if (object != undefined)
            this.Objects.push(object);
    }



}