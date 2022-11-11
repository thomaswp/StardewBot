var Commands = {};
var Interpreters = {};

function createWebsocket() {
    var socket = new WebSocket("ws://127.0.0.1:8000");

    socket.onopen = function (event) {

    };

    socket.onmessage = function (event) {
        console.log(event.data);

        let message = event.data;
        if (message.length < 2 || message[0] !== '{' || message[message.length - 1] != '}') {
            console.log(message);
        }

        try {
            let command = JSON.parse(message);
            if (Commands[command.type]) {
                Commands[command.type](command.data);
            } else {
                console.log("Unknown comamnd: " + command.type);
            }
        } catch (e) {
            console.log(e, message);
        }
    }

    socket.onclose = function(event) {
        Target.reset();
        // setTimeout(() => {
        //     console.log('Retrying');
        //     window.socket = createWebsocket();
        // }, 5000);
    }

    return socket;
}

window.socket = createWebsocket();

window.onbeforeunload = function() {
    if (!window) return;
    window.socket.send("Disconnect");
}

Commands['SetTarget'] = (data) => {
    if (Target.currentEditing) {
        Target.currentEditing.unsetAsEditing();
    }
    Target.getTarget(data.targetID, data.targetName, data.code, data.varMap).setAsEditing();
};

Commands['SyncCode'] = (data) => {
    console.log('sync', data);
    data.forEach(robot => {
        // console.log(robot.Guid, robot.Code);
        Target.getTarget(robot.Guid, null, robot.Code, robot.VarMap);
    });
    // Target.getTarget(data.targetID, data.targetName, data.code)
};

Commands['TriggerEvent'] = (data) => {
    Target.getTarget(data.targetID).trigger(data.eventName);
};

Commands['BlockFinished'] = (data) => {
    // console.log('BlockFinished', data);
    Target.getTarget(data.targetID).blockCallback(data.threadID, data.returnValue);
};

Commands['DefineBlocks'] = (data) => {
    if (!window.blocklyConstructor) {
        window.blocklyConstructor = new BlocklyConstructor();
    }
    window.blocklyConstructor.defineBlocks(data);
};

Commands['Resize'] = (data) => {
    const container = document.getElementById("container");
    if (!container) return;
    container.style.width = data.width + "px";
    container.style.height = data.height + "px";
    Blockly.svgResize(window.mainWorkspace);

    // TODO: No constant...
    const testButton = document.getElementById("test-button");
    testButton.style.left = (data.width - 165) + "px";
    testButton.style.right = "";
};
