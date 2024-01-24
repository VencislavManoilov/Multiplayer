const WebSocket = require('ws');
const { v4: uuidv4 } = require('uuid');
const PORT = 3000;

const server = new WebSocket.Server({ port: PORT });

let sendData;

server.on('connection', (websocket) => {
    
    const clientId = uuidv4();
    websocket.clientId = clientId;
    websocket.position = {x: randomInteger(0, 10), y: 10, z: randomInteger(0, 10)};
    websocket.rotation = {x: 0, y: 0, z: 0};
    websocket.health = 100;
    
    sendToEveryoneElse(clientId, JSON.stringify({type: "newPlayerJoinedLobby", id: clientId, position: websocket.position, rotation: websocket.rotation, health: websocket.health}));
    
    console.log('Client connected: ' + websocket.clientId);

    // Listen for messages from the client
    websocket.on('message', (message) => {

        let data = JSON.parse(message);
        
        switch (data.type) {
            case "position":
                sendData = {type: "position", who: websocket.clientId, position: data.position, rotation: data.rotation};
                sendToEveryoneElse(websocket.clientId, JSON.stringify(sendData));
                break;
            case "health":
                const targetClient = getClientById(data.id);

                if(targetClient) {
                    targetClient.health = data.health;

                    sendData = {type: "health", who: data.id, health: data.health};
                    sendToEveryoneElse(websocket.clientId, JSON.stringify(sendData));
                } else {
                    console.log(`Client with id ${data.id} not found.`);
                }

                break;
            case "lobby":
                let otherIds = [], otherPositions = [], otherHeath = [];
                server.clients.forEach((client) => {
                    if (client.clientId !== websocket.clientId) {
                        otherIds.push(client.clientId);
                        otherPositions.push({x: client.position.x, y: client.position.y, z: client.position.z});
                        otherHeath.push(client.health);
                    }
                });

                sendData = {type: "lobby", myId: websocket.clientId, myPosition: websocket.position, otherIds: otherIds, otherPositions: otherPositions, health: otherHeath};
                sendToOne(websocket.clientId, JSON.stringify(sendData));
                break;
            default:
                console.log("What is this: " + data.type);
                break;
        }
    });

    // Handle disconnection
    websocket.on('close', () => {
        sendToEveryoneElse(websocket.clientId, JSON.stringify({type: "playerLeftLobby", id: websocket.clientId}));

        console.log('Client disconnected: ' + websocket.clientId);
    });
});

function sendToAll(message) {
    server.clients.forEach((client) => {
        if (client.readyState === WebSocket.OPEN) {
            client.send(`Server broadcast: ${message}`);
        }
    });
}

function sendToOne(who, message) {
    server.clients.forEach((client) => {
        if (client.readyState === WebSocket.OPEN && client.clientId === who) {
            client.send(message);
        }
    });
}

function sendToEveryoneElse(whoNot, message) {
    server.clients.forEach((client) => {
        if (client.readyState === WebSocket.OPEN && client.clientId !== whoNot) {
            client.send(message);
        }
    });
}

function getClientById(clientId) {
    return [...server.clients].find((client) => client.clientId === clientId);
}

function randomInteger(min, max) {
    return Math.floor(Math.random() * max) + min;
}

console.log('WebSocket server started at ws://localhost:3000');