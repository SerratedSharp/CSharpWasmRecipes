
let EventsShim = {}; //globalThis.EventsShim || {};
(function (EventsShim) {

    EventsShim.SubscribeEventById = function (elementId, eventName, listenerFunc) {
        const elementObj = document.getElementById(elementId);

        // Need to wrap the Managed C# action in JS func (only because it is being returned)
        let handler = function (event) {            
            listenerFunc(event.type, event.target.id);// decompose object to primitives
        }.bind(elementObj);        

        elementObj.addEventListener(eventName, handler, false);        
        return handler;// return JSObject reference so it can be used for removeEventListener later
    }

    // Param listenerHandler must be the JSObject reference returned from the prior SubscribeEvent call
    EventsShim.UnsubscribeEventById = function (elementId, eventName, listenerHandler) {
        const elementObj = document.getElementById(elementId);
        elementObj.removeEventListener(eventName, listenerHandler, false);
    }

    EventsShim.TriggerClick = function (elementId) {
        const elementObj = document.getElementById(elementId);
        elementObj.click();
    }

    EventsShim.GetElementById = function (elementId) {
        return document.getElementById(elementId);
    }

    EventsShim.SubscribeEvent = function (elementObj, eventName, listenerFunc) {
        // Need to wrap the Managed C# action in JS func
        let handler = function (e) {
            listenerFunc(e);
        }.bind(elementObj);

        elementObj.addEventListener(eventName, handler, false);
        return handler;// return JSObject reference so it can be used for removeEventListener later
    }

    EventsShim.UnsubscribeEvent = function (elementObj, eventName, listenerHandler) {        
        return elementObj.removeEventListener(eventName, listenerHandler, false);
    }

    // TODO: Move to troubleshooting
    EventsShim.SubscribeEventFailure = function (elementObj, eventName, listenerFunc) {
        // It's not strictly required to wrap the C# action listenerFunc in a JS function.
        elementObj.addEventListener(eventName, listenerFunc, false);
        // However, if you need to return the wrapped proxy object you will get an error when it tries to wrap the existing proxy in an additional proxy:
        return listenerFunc; // Error: "JSObject proxy of ManagedObject proxy is not supported."
    }

    EventsShim.SubscribeEventByIdWithLogging = function (elementId, eventName, listenerFunc) {
        const elementObj = document.getElementById(elementId);
        
        // Need to wrap the Managed C# action in JS func (only because it is being returned)
        let handler = function (event) {            
            listenerFunc(event.type, event.target.id);// decompose object to primitives
        }.bind(elementObj);        

        elementObj.addEventListener(eventName, handler, false);

        return handler;// return JSObject reference so it can be used for removeEventListener later
    }
    
})(EventsShim);

export { EventsShim };