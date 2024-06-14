

let PromisesShim = {}; //globalThis.PromisesShim || {};
(function (PromisesShim) {

    PromisesShim.Wait2Seconds = function () {
        // This also demonstrates wrapping a callback-based API in a promise to make it awaitable.
        // This is only appropriate for single-shot operations. Events should be used if a callback might be called multiple times.
        return new Promise((resolve, reject) => {
            setTimeout(() => {                
                resolve();// resolve promise after 2 seconds
            }, 2000);
        });
    };
    
    // Returning a value via resolve() in a promise
    PromisesShim.WaitGetString = function () {
        return new Promise((resolve, reject) => {
            setTimeout(() => {
                resolve("String From Resolve");// return a string via promise
            }, 500);
        });
    };

    PromisesShim.WaitGetDate = function () {        
        return new Promise((resolve, reject) => {
            setTimeout(() => {
                resolve(new Date('1988-11-24'))// return a date via promise
            }, 500);
        });
    };

    // awaitable fetch()
    PromisesShim.FetchCurrentUrl = function () {
        // We return the promise returned from .then()
        // and .NET can in turn await the returned promise.
        return fetch(globalThis.window.location, { method: 'GET' })
            .then(response => response.text());
    };

    // .NET can await JS functions using the async/await JS syntax:
    PromisesShim.AsyncFunction = async function () {
        await PromisesShim.Wait2Seconds();
    };

    PromisesShim.ConditionalSuccess = function (shouldSucceed) {        
       
        return new Promise((resolve, reject) => {
            setTimeout(() => {
                if (shouldSucceed)                
                    resolve();// success
                else                     
                    reject("Reject: ShouldSucceed == false");// failure
            }, 500);

        });

    };

    PromisesShim.WaitGetIntArrayAsObject = function () {
        return new Promise((resolve, reject) => {
            setTimeout(() => {
                resolve([1, 2, 3, 4, 5] );
            }, 500);
        });
    };

    PromisesShim.UnwrapJSObjectAsIntArray = function (jsObject) {
        return jsObject;
    };

})(PromisesShim);

export { PromisesShim };