var Commands = {};
var Interpreters = {};
var explanation = {};
var explanationPage = 0;

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

function saveTutorial() {
	if(!window.socket) return;
	window.socket.send(JSON.stringify({
		type: 'saveTutorial',
		data: null
	}));
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
    // container.style.width = data.width + "px";
    // container.style.height = data.height + "px";
    Blockly.svgResize(window.mainWorkspace);

    // TODO: No constant...
    const testButton = document.getElementById("test-button");
    // testButton.style.left = (data.width - 165) + "px";
    // testButton.style.right = "";
};

Commands['LoadTutorial'] = (data) => {
	explanationPage = 0;
	var entry = data.explanation[explanationPage];
    explanation = data.explanation;
	
	console.log(explanation);
	if(entry) {
		document.getElementById("panelWrapper").innerHTML = entry;
	} else {
		document.getElementById("panelWrapper").innerHTML = "No Explanation Text Provided.";
	}
	
	document.getElementById("headerWrapper").innerHTML = "Level Explanation (" + data.levelName + ")";
	
	if(explanation.length > 1) {
		document.getElementById("explanationRight").classList.add("display");
	}
	
	document.getElementById("explanation").classList.add("display");
};

Commands['EndTutorial'] = (data) => {
	document.getElementById("explanation").classList.remove("display");
	document.getElementById("panelWrapper").innerHTML = "";
	document.getElementById("headerWrapper").innerHTML= "Level Explanation (";
	document.getElementById("explanationLeft").classList.remove("display");
	document.getElementById("explanationRight").classList.remove("display");
};
