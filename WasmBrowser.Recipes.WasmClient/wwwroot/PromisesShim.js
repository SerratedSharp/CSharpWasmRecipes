PromisesShim.Wait2Seconds = function () {
    // This also demonstrates wrapping a callback-based API in a promise to 
    // make it awaitable.
    return new Promise((resolve, reject) => {
        setTimeout(() => {
            resolve(); // Resolve promise after 2 seconds
        }, 2000);
    });
};

// Return a value via resolve in a promise.
PromisesShim.WaitGetString = function () {
    return new Promise((resolve, reject) => {
        setTimeout(() => {
            resolve("String From Resolve"); // Return a string via promise
        }, 500);
    });
};

PromisesShim.WaitGetDate = function () {
    return new Promise((resolve, reject) => {
        setTimeout(() => {
            resolve(new Date('1988-11-24')) // Return a date via promise
        }, 500);
    });
};

// Demonstrates an awaitable fetch.
PromisesShim.FetchCurrentUrl = function () {
    // This method returns the promise returned by .then(*.text())
    // and .NET awaits the returned promise.
    return fetch(globalThis.window.location, { method: 'GET' })
        .then(response => response.text());
};

// .NET can await JS methods using the async/await JS syntax.
PromisesShim.AsyncFunction = async function () {
    await PromisesShim.Wait2Seconds();
};

// A Promise.reject can be used to signal failure and is bubbled to .NET code 
// as a JSException.
PromisesShim.ConditionalSuccess = function (shouldSucceed) {
    return new Promise((resolve, reject) => {
        setTimeout(() => {
            if (shouldSucceed)
                resolve(); // Success
            else
                reject("Reject: ShouldSucceed == false"); // Failure
        }, 500);
    });
};