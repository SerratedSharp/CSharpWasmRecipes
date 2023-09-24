using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uno.Foundation;

namespace UnoBootstrap.Recipes.WasmClient.Basic
{

    // IMPORTANT: Invoke Async requires the package Uno.UI.WebAssembly due to dependency on JS declaration of Uno.UI.Interop
    // Promises implemented with Uno.Foundation.WebAssemblyRuntime
    internal class PromisesWUno
    {
        // For functions which already return a promise, this allows C# to await the promise.  From: https://platform.uno/docs/articles/interop/wasm-javascript-1.html#invoke-javascript-code-from-c
        public static async Task AwaitFunctionReturningPromise()
        {
            await WebAssemblyRuntime.InvokeAsync("""
                return fetch('https://api.example.com/documents/1301', {method: 'DELETE'});
            """);
        }

        // We can't return the entire response object across WASM, so we access the .text() and return that.
        // This works because the second .then also returns a promise since .text() returns a promise that resolves to a string: https://developer.mozilla.org/en-US/docs/Web/API/Response/text
        public static async Task<string> AwaitFunctionReturningPromisedString()
        {
            return await WebAssemblyRuntime.InvokeAsync("""
                fetch(window.location, {method: 'GET'})  // request current URL
                .then(response => response.text()) // this .then itself returns a promise with a string return
                ;
            """);
        }

        // For async JS functions, to allow C# to await.  From: https://platform.uno/docs/articles/interop/wasm-javascript-1.html#invoke-javascript-code-from-c  
        // Also demonstrates returning a string from JS async to C#.
        public static async Task<string> AwaitJSAsync()
        {
            var str = await WebAssemblyRuntime.InvokeAsync("""
                (async () => "It works asynchronously!")();
            """);
            return str;// "It works asynchronously!"
        }

        // Converts a JS callback to an async Promise.  Used when subsequent .NET code must await a JS callback.
        public static async Task ConvertCallbackToPromise()
        {
            await WebAssemblyRuntime.InvokeAsync("""
              new Promise((resolve, reject) => {
                  console.log("Time from JS Before:", new Date().toLocaleTimeString());
                  setTimeout(() => {
                    console.log("Time from JS in Callback:", new Date().toLocaleTimeString());
                    return resolve();
                  }, 3000);
              });              
            """);
        }

        // A variation that returns a string via resolve("string to return")
        public static async Task<string> ConvertCallbackToPromisedString()
        {
            return await WebAssemblyRuntime.InvokeAsync("""
              new Promise((resolve, reject) => {
                  setTimeout(() => {
                    return resolve("string to return");
                  }, 3000);
              });
            """);
        }

        // In most uses of InvokeAsync, there is a single statement that resolves to a promise, and no return statement.
        // If additional code proceeding the promise is needed, then it must be enclosed in {} block and use `return` to return the Promise from the block.
        public static async Task<string> MultilinePromise()
        {
            return await WebAssemblyRuntime.InvokeAsync("""
            {
              console.log("Code proceeding promise");

              return new Promise((resolve, reject) => {
                  setTimeout(() => {
                    return resolve("string to return");
                  }, 3000);
              });
            }
            """);
        }

        // Used when subsequent code dependends on a RequireJS dependency and must wait for resolution.
        public static async Task RequireJSAndAwaitDependencyLoading()
        {
            WebAssemblyRuntime.InvokeJS("""
                requirejs.config({
                    paths: {
                        popper: 'https://unpkg.com/@popperjs/core@2.11.8/dist/umd/popper.min'    
                    }
                });
            """);

            // When a promise is implemented as a single statement, no enclosing braces { } nor `return` statement is needed. The return is implicit.
            await WebAssemblyRuntime.InvokeAsync("""
                new Promise((resolve, reject) => {
                    require(["popper"], (popper) => {
                        globalThis.Popper = popper
                        resolve();// allow caller to await resolution of dependency
                    });
                });      
            """);
        }

        // Used when subsequent code depends on a RequireJS dependency and must wait for resolution.
        public static async Task RequireAndAwaitDependencyLoadingMultiline()
        {
            // Calling InvokeAsync with mult-line statements requires enclosing braces { } and a `return` statement.            
            await WebAssemblyRuntime.InvokeAsync("""  
                {
                    requirejs.config({
                    paths: {
                        popper: 'https://unpkg.com/@popperjs/core@2.11.8/dist/umd/popper.min'    
                        }
                    });
                    return new Promise((resolve, reject) => {
                        require(["popper"], (popper) => {
                            globalThis.Popper = popper
                            resolve();// allow caller to await resolution of dependency
                        });
                    });
                }
            """);
        }




    }
}
