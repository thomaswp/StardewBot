class Thread {
    
    constructor(target, triggeringBlock) {
        this.target = target;
        this.interpreter = null;
        this.callback = null;
        if (triggeringBlock) {
            this.triggeringEvent = triggeringBlock.type;
            this.triggeringBlockID = triggeringBlock.id;
        }
        this.id = Thread.uuidv4();
    }

    static uuidv4() {
        return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
            var r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
            return v.toString(16);
        });
    }

    blockCallback(returnValue) {
        if (!this.callback) {
            console.warn("Unset block callback for thread: " + this.id);
            return;
        }
        if (this.target.isEditing) {
            highlightBlock(null);
        }

        console.log("Callback with return: ", returnValue);
        this.callback(returnValue);
        this.callback = null;
        this.step();
    }


    step() {
        console.log(`Stepping thread: ${this.id}`);
        var hasMore = this.interpreter.run();
        if (!hasMore) this.die();
    }

    die() {
        this.target.removeThread(this.id);
    }

    run(code) {
        let thread = this;
        function initApi(interpreter, globalObject) {      
            interpreter.setProperty(globalObject, BlocklyConstructor.GET_VAR,
                interpreter.createNativeFunction(function(name) {
                    return thread.target.getVar(name);
                }
            ));

            interpreter.setProperty(globalObject, BlocklyConstructor.SET_VAR,
                // TODO: This won't work with non-primitives,, e.g. lists...
                interpreter.createNativeFunction(function(name, value) {
                    thread.target.setVar(name, value);
                }
            ));

            interpreter.setProperty(globalObject, BlocklyConstructor.CALL_BLOCK,
                interpreter.createAsyncFunction(function(
                        // Silly hack to allow any number of arguments to be passed to the same function
                        name, id, nArgs,
                        a2, a3, a4, a5, a6, a7, a8, a9, a10, 
                        a11, a12, a13, a14, a15, 
                        callback
                    ) {
                        if (thread.target.isEditing) {
                            highlightBlock(id);
                        }
                        // console.log('args', arguments);
                        const nPreArgs = 3;
                        var args = [...arguments].slice(nPreArgs, nArgs + nPreArgs);
                        console.log('Calling: ', name, args);
                        Thread.callBlock({
                            methodName: name,
                            args: args,
                            targetID: thread.target.id,
                            threadID: thread.id,
                        });
                        thread.callback = callback;
                    }
                )
            );
      
            // Add an API function for highlighting blocks.
            var wrapper = function(id) {
              id = String(id || '');
              highlightBlock(id);
            };
            interpreter.setProperty(globalObject, BlocklyConstructor.HIGHLIGHT,
                interpreter.createNativeFunction(wrapper));
        }
        
        this.interpreter = new Interpreter(code, initApi);
        // TODO: Should step so its interruptable
        this.step();
    }

    static callBlock(data) {
        console.log('Calling', data);
        window.socket.send(JSON.stringify({
            'type': 'call',
            'data': data,
        }));

    }
}