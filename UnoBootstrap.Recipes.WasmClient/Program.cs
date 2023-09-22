using System;
using System.IO;
using System.Runtime.InteropServices.JavaScript;
using System.Threading;
using System.Threading.Tasks;
using Uno.Foundation;

using JSConsole = UnoBootstrap.Recipes.WasmClient.JSWrappers.Console;


namespace UnoBootstrap.Recipes.WasmClient
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            // NOTE: Because this code is running in the browser, Console.WriteLine() appears in the browser's debug console.
            Console.WriteLine("Hello, World!");

            // We can await values returned from JS promises:
            string resolvedValue = await Basic.Promises.AwaitFunctionReturningPromisedString();
            Console.WriteLine(resolvedValue);

            // We can also convert JS callbacks to awaitable promises
            await Basic.Promises.ConvertCallbackToPromise();

            // Note: Our javascript Console wrapper is aliased at the top using as JSConsole to avoid conflict with System.Console.
            JSConsole.Log("LoggedViaWrapper", DateTime.Now);

            // Call console.log a different way using JSImport
            Basic.JSImportExample.GlobalThisConsoleLog("Hello from JSImported console.log!");

            // Calling our examples of returning primitive values from JS using InvokeJS:
            Console.WriteLine("HasFocus: " + JSWrappers.Document.HasFocus());
            Console.WriteLine("HasFocus as JS bool: " + JSWrappers.Document.HasFocusWithJSParsedType());

            // Declare static JS functions to act as proxies, which will then be mapped with JSImport.  (Could have been included/loaded using js/typescript instead)
            WebAssemblyRuntime.InvokeJS("""
                globalThis.findElement = function(id) { return document.getElementById(id); }
                globalThis.getClass = function(obj) { return obj.getAttribute('class'); }
                """);
            // Note findElement returns an object, and getClass takes an object as a parameter.
            // This corresponds with the JSImport's method declarations using JSObject in JSObjectExample.cs

            // JSImport can return object references and primitive values.
            // Call the static JS functions from C#:
            JSObject element = Basic.JSObjectExample.FindElement("uno-body");
            // Pass the handle to another method that calls in instance method on the object:
            var elementClasses = Basic.JSObjectExample.GetClass(element);
            Console.WriteLine("Class string: " + elementClasses);



        }
    }

}