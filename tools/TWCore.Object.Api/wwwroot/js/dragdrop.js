function addEventHandler(obj, evt, handler) {
    if (obj.addEventListener) {
        // W3C method
        obj.addEventListener(evt, handler, false);
    } else if (obj.attachEvent) {
        // IE method.
        obj.attachEvent('on' + evt, handler);
    } else {
        // Old school method.
        obj['on' + evt] = handler;
    }
};
Function.prototype.bindToEventHandler = function bindToEventHandler() {
    var handler = this;
    var boundParameters = Array.prototype.slice.call(arguments);
    //create closure
    return function (e) {
        e = e || window.event; // get window.event if e argument missing (in IE)   
        boundParameters.unshift(e);
        handler.apply(this, boundParameters);
    }
};


window.droppedHandlers = [];
window.addOnDropFileHandler = function (fn) {
    window.droppedHandlers.push(fn);
};

if (window.FileReader) {
    addEventHandler(window, 'load', function () {
        var drop = document.body;

        function cancel(e) {
            if (e.preventDefault) { e.preventDefault(); }
            return false;
        }

        // Tells the browser that we *can* drop on this target
        addEventHandler(drop, 'dragover', cancel);
        addEventHandler(drop, 'dragenter', cancel);
        addEventHandler(drop, 'drop', function (e) {
            e = e || window.event; // get window.event if e argument missing (in IE)   
            if (e.preventDefault) { e.preventDefault(); } // stops the browser from redirecting off to the image.

            var dt = e.dataTransfer;
            var files = dt.files;
            for (var i = 0; i < files.length; i++) {
                var file = files[i];
                if (file.size > 2097152) 
                    continue;
                var reader = new FileReader();

                addEventHandler(reader, 'loadend', function (e, file) {
                    var bin = this.result;
                    for (var m = 0; m < window.droppedHandlers.length; m++)
                        window.droppedHandlers[m](file.name, bin);
                }.bindToEventHandler(file));

                reader.readAsArrayBuffer(file);
            }
            return false;
        });
    });
}