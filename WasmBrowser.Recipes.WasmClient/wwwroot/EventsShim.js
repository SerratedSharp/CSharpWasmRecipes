EventsShim.subscribeEventById = function (elementId, eventName, listenerFunc) {
    const elementObj = document.getElementById(elementId);

    // Need to wrap the Managed C# action in JS func (only because it is being 
    // returned).
    let handler = function (event) {
        listenerFunc(event.type, event.target.id); // Decompose object to primitives
    }.bind(elementObj);

    elementObj.addEventListener(eventName, handler, false);
    // Return JSObject reference so it can be used for removeEventListener later.
    return handler;
}

// Param listenerHandler must be the JSObject reference returned from the prior 
// SubscribeEvent call.
EventsShim.unsubscribeEventById = function (elementId, eventName, listenerHandler) {
    const elementObj = document.getElementById(elementId);
    elementObj.removeEventListener(eventName, listenerHandler, false);
}

EventsShim.triggerClick = function (elementId) {
    const elementObj = document.getElementById(elementId);
    elementObj.click();
}

EventsShim.getElementById = function (elementId) {
    return document.getElementById(elementId);
}

EventsShim.subscribeEvent = function (elementObj, eventName, listenerFunc) {
    let handler = function (e) {
        listenerFunc(e);
    }.bind(elementObj);

    elementObj.addEventListener(eventName, handler, false);
    return handler;
}

EventsShim.unsubscribeEvent = function (elementObj, eventName, listenerHandler) {
    return elementObj.removeEventListener(eventName, listenerHandler, false);
}

EventsShim.subscribeEventFailure = function (elementObj, eventName, listenerFunc) {
    // It's not strictly required to wrap the C# action listenerFunc in a JS 
    // function.
    elementObj.addEventListener(eventName, listenerFunc, false);
    // If you need to return the wrapped proxy object, you will receive an error 
    // when it tries to wrap the existing proxy in an additional proxy:
    // Error: "JSObject proxy of ManagedObject proxy is not supported."
    return listenerFunc;
}