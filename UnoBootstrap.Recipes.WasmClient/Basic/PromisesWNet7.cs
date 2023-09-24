using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Threading.Tasks;
using Uno.Foundation;

namespace UnoBootstrap.Recipes.WasmClient.Basic
{

    // Promises implemented with .NET 7 System.Runtime.InteropServices.Javascript
    public partial class PromisesWNet7
    {

        public static void DeclareJSFunctionReturningPromisedString()
        {
            // We could have instead declared this JS in a *.js/*.ts file and placed in a more appropriate namespace
            // The function returns a Promise that resolves to a string, and takes a single string param
            WebAssemblyRuntime.InvokeJS("""
                globalThis.functionReturningPromisedString = function (url) {   
                    return fetch(url, {method: 'GET'})  // request current URL
                        .then(response => response.text()); // .then returns a promise with a string return, note this is because .text() is also async
                }   
            """);
        }

        // MAtch the above javascript function signature.
        [JSImport("globalThis.functionReturningPromisedString")]
        [return: JSMarshalAs<JSType.Promise<JSType.String>>()] // JS function returns a promise that resolves to a string
        public static partial Task<string> // the return type Task<string> matches the Promise<string>
            FunctionReturningPromisedString(string url);

        //// For async JS functions, to allow C# to await.  From: https://platform.uno/docs/articles/interop/wasm-javascript-1.html#invoke-javascript-code-from-c  
        //// Also demonstrates returning a string from JS async to C#.
        //public static async Task<string> AwaitJSAsync()
        //{
        //    var str = await WebAssemblyRuntime.InvokeAsync("""
        //        (async () => "It works asynchronously!")();
        //    """);
        //    return str;// "It works asynchronously!"
        //}

        //// Converts a JS callback to an async Promise.  Used when subsequent .NET code must await a JS callback.
        //public static async Task ConvertCallbackToPromise()
        //{
        //    await WebAssemblyRuntime.InvokeAsync("""
        //      new Promise((resolve, reject) => {
        //          console.log("Time from JS Before:", new Date().toLocaleTimeString());
        //          setTimeout(() => {
        //            console.log("Time from JS in Callback:", new Date().toLocaleTimeString());
        //            return resolve();
        //          }, 3000);
        //      });              
        //    """);
        //}

        //// A variation that returns a string via resolve("string to return")
        //public static async Task<string> ConvertCallbackToPromisedString()
        //{
        //    return await WebAssemblyRuntime.InvokeAsync("""
        //      new Promise((resolve, reject) => {
        //          setTimeout(() => {
        //            return resolve("string to return");
        //          }, 3000);
        //      });
        //    """);
        //}

        //// In most uses of InvokeAsync, there is a single statement that resolves to a promise, and no return statement.
        //// If additional code proceeding the promise is needed, then it must be enclosed in {} block and use `return` to return the Promise from the block.
        //public static async Task<string> MultilinePromise()
        //{
        //    return await WebAssemblyRuntime.InvokeAsync("""
        //    {
        //      console.log("Code proceeding promise");

        //      return new Promise((resolve, reject) => {
        //          setTimeout(() => {
        //            return resolve("string to return");
        //          }, 3000);
        //      });
        //    }
        //    """);
        //}

        //// Used when subsequent code dependends on a RequireJS dependency and must wait for resolution.
        //public static async Task RequireJSAndAwaitDependencyLoading()
        //{
        //    WebAssemblyRuntime.InvokeJS("""
        //        requirejs.config({
        //            paths: {
        //                popper: 'https://unpkg.com/@popperjs/core@2.11.8/dist/umd/popper.min'    
        //            }
        //        });
        //    """);

        //    // When a promise is implemented as a single statement, no enclosing braces { } nor `return` statement is needed. The return is implicit.
        //    await WebAssemblyRuntime.InvokeAsync("""
        //        new Promise((resolve, reject) => {
        //            require(["popper"], (popper) => {
        //                globalThis.Popper = popper
        //                resolve();// allow caller to await resolution of dependency
        //            });
        //        });      
        //    """);
        //}

        //// Used when subsequent code depends on a RequireJS dependency and must wait for resolution.
        //public static async Task RequireAndAwaitDependencyLoadingMultiline()
        //{
        //    // Calling InvokeAsync with mult-line statements requires enclosing braces { } and a `return` statement.            
        //    await WebAssemblyRuntime.InvokeAsync("""  
        //        {
        //            requirejs.config({
        //            paths: {
        //                popper: 'https://unpkg.com/@popperjs/core@2.11.8/dist/umd/popper.min'    
        //                }
        //            });
        //            return new Promise((resolve, reject) => {
        //                require(["popper"], (popper) => {
        //                    globalThis.Popper = popper
        //                    resolve();// allow caller to await resolution of dependency
        //                });
        //            });
        //        }
        //    """);
        //}




    }
}
