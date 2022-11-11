function highlightBlock(id) {
    window.mainWorkspace.highlightBlock(id);
}

class Target {

    static targets = {};
    static currentEditing = null;

    static getTarget(id, name, code, varMap) {
        let target = this.targets[id];
        if (target) {
            if (code) target.code = Blockly.Xml.textToDom(code);
            if (name) target.name = name;
            return target;
        }
        
        this.targets[id] = new Target(id, name, code, varMap);
        return this.targets[id];
    }

    static reset() {
        if (this.currentEditing) this.currentEditing.unsetAsEditing();
        this.targets = {};
        this.currentEditing = null;
    }

    constructor(id, name, code, varMap) {
        this.id = id;
        this.name = name;
        
        if (code) {
            this.code = Blockly.Xml.textToDom(code)
        } else {
            this.code = document.getElementById('startBlocks');
        }

        if (varMap) {
            try {
                // console.log('setting varMap', varMap);
                this.varMap = new Map(Object.entries(JSON.parse(varMap)));
                // console.log('success', this.varMap);
            } catch {
                console.warn("Failed to init var map with", varMap);
            }
        } 
        if (!varMap) {
            this.varMap = new Map();
        }

        this.threads = {};
        this.isEditing = false;
        this.updateListener = event => this.onUpdate(event);
        this.saveCallback = null;

        this.workspace = new Blockly.Workspace();
        Blockly.Xml.domToWorkspace(this.code, this.workspace);
    }

    getVar(name) {
        console.log(`Getting ${name} for ${this.name}: ${this.varMap.get(name)}`);
        return this.varMap.get(name) | null;
    }

    setVar(name, value) {
        console.log(`Setting ${name} for ${this.name} to: ${value}`);
        this.varMap.set(name, value);
    }

    onUpdate(event) {
        this.saveCodeDelayed();
    }

    unsetAsEditing() {
        this.saveCode();
        this.isEditing = false;
        window.mainWorkspace.removeChangeListener(this.updateListener);
    }

    saveCodeDelayed() {
        clearTimeout(this.saveCallback);
        this.saveCallback = setTimeout(() => this.saveCode(), 2000);
    }

    saveCode() {
        clearTimeout(this.saveCallback);
        if (!this.isEditing) return;
        this.code = Blockly.Xml.workspaceToDom(window.mainWorkspace);
        this.workspace.clear();
        Blockly.Xml.domToWorkspace(this.code, this.workspace);
        const data = {
            'type': 'save',
            'data': {
                targetID: this.id,
                code: this.code ? this.code.outerHTML : null,
                varMap: JSON.stringify(Object.fromEntries(this.varMap)),
            },
        };
        // console.log('saving', data);
        window.socket.send(JSON.stringify(data));
    }

    setAsEditing() {
        this.isEditing = true;
        Target.currentEditing = this;
        window.mainWorkspace.clear();
        Blockly.Xml.domToWorkspace(this.code, window.mainWorkspace);

        window.mainWorkspace.addChangeListener(this.updateListener);
    }

    trigger(name) {
        this.saveCode();
        // Threads already running for this event that shouldn't be re-run
        var runningBlocks = []
        if (!window.blocklyConstructor.isEventStackable(name)) {
            Object.keys(this.threads).forEach(threadID => {
                let thread = this.threads[threadID];
                if (thread.triggeringEvent == name) {
                    runningBlocks.push(thread.triggeringBlockID);
                }
            });
        }

        // console.log(this.isEditing);
        // console.log(blocks);
        // console.log(Blockly.Xml.domToText(Blockly.Xml.workspaceToDom(this.workspace)));

        // console.log(Blockly.JavaScript.workspaceToCode(this.workspace));

        // Collect all custom block definitions so we can add them to each thread we create
        let customBlocks = [];
        Blockly.Procedures.allProcedures(this.workspace).forEach(blocks => {
            blocks.forEach(block => {
                const procBlock = Blockly.Procedures.getDefinition(block[0], this.workspace);
                customBlocks.push(procBlock);
            });
        });
        
        let blocks = this.workspace.getBlocksByType(name);
        blocks.forEach(block => {
            // Skip already running event blocks
            if (runningBlocks.includes(block.id)) return;
            Blockly.JavaScript.init(this.workspace);
            // console.log(Blockly.JavaScript.nameDB_, Blockly.JavaScript.definitions_);
            let  code = Blockly.JavaScript.blockToCode(block);
            // Call blockToCode on each procedure and then finish to generate their code
            // Note that blockToCode generates no code on call, but it adds procedures to the DB
            customBlocks.forEach(proc => Blockly.JavaScript.blockToCode(proc));
            code = Blockly.JavaScript.finish(code)
            
            let thread = new Thread(this, block);
            console.log(`Running ${code} on thread ${thread.id}`);
            this.threads[thread.id] = thread;
            thread.run(code);
        });
    }

    blockCallback(threadID, returnValue) {
        if (!this.threads[threadID]) {
            console.log(`Call to dead thread: ${threadID}`);
            return;
        }
        this.threads[threadID].blockCallback(returnValue)
    }

    step() {

    }

    removeThread(id) {
        delete this.threads[id];
    }

    
}