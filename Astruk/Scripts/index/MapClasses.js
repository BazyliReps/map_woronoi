class Vertex {
    constructor(id, x, y) {
        this.Id = id;
        this.X = x;
        this.Y = y;
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
    constructor(name, type) {
        this.Name = name;
        this.Type = type;
    }

}

class MapObjectType {
    constructor(id, typeName) {
        this.Id = id;
        this.Name = typeName;
        this.Parameters = [];
    }

    addPair(fieldName, type) {
        this.Parameters.push(new Pair(fieldName, type));
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