using System;
using System.IO;
using System.Runtime.InteropServices.JavaScript;
using System.Threading;
using System.Threading.Tasks;
using Uno.Foundation;
using UnoBootstrap.Recipes.WasmClient.Basic;

// Alias our JS console wrapper to avoid conflicts with System.Console
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
            string resolvedValue = await Basic.PromisesWUno.AwaitFunctionReturningPromisedString();
            Console.WriteLine(resolvedValue);

            // We can also convert JS callbacks to awaitable promises
            await Basic.PromisesWUno.ConvertCallbackToPromise();

            Basic.PromisesWNet7.DeclareJSFunctionReturningPromisedString();
            var fetchBody = await Basic.PromisesWNet7.FunctionReturningPromisedString("https://cat-fact.herokuapp.com/facts/");
            Console.WriteLine("fetchBody: " + fetchBody.Substring(0,100) + "...");

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
                globalThis.getClass = function(elementObj) { return elementObj.getAttribute('class'); }
                globalThis.subscribeEvent = function(elementObj, eventName, listenerFunc) { 
                    return elementObj.addEventListener( eventName, listenerFunc, false ); 
                } 
                """);

            // Note findElement returns an object, and getClass takes an object as a parameter.
            // This corresponds with the JSImport's method declarations using JSObject in JSObjectExample.cs

            // JSImport can return object references and primitive values.
            // Call the static JS functions from C#:
            JSObject element = Basic.JSObjectExample.FindElement("uno-body");
            // Pass the handle to another method that calls in instance method on the object:
            var elementClasses = Basic.JSObjectExample.GetClass(element);
            Console.WriteLine("Class string: " + elementClasses);

            Basic.JSObjectExample.SusbcribeEvent(element, "click", (eventObj) => {
                Console.WriteLine($"Event fired with event type via interop property '{eventObj.GetPropertyAsString("type")}'");
                JSObjectExample.Log(eventObj);
            });

            // Using WebAssemblyRuntime based instance wrapper 
            Advanced.ElementWrapper elementWrapper = Advanced.ElementWrapper.GetElementById("uno-body");
            var elementClasses2 = elementWrapper.GetClass();
            Console.WriteLine("Class string2: " + elementClasses2);

            JSObject jsObj = JSObjectExample.GetJsonAsJSObject(
                """
                {
                    "firstName":"Crow",
                    "middleName":"T",
                    "lastName":"Robot",
                    "innerObj":
                    {
                        "prop1":"innerObj Prop1 Value",
                        "prop2":"innerObj Prop2 Value"
                    }
                }
                """);

            // Store a reference in JS globalThis to dmeonstrate by ref modification
            JSHost.GlobalThis.SetProperty("jsObj", jsObj); 

            JSObjectExample.ConsoleLogJSObject(jsObj);
            string lastName = jsObj.GetPropertyAsString("lastName");
            Console.WriteLine("LastName: " + lastName);
            Console.WriteLine("Type: " + jsObj.GetType());
            Console.WriteLine("lastName Type: " + jsObj.GetTypeOfProperty("lastName"));

            Console.WriteLine("innerObj Type: " + jsObj.GetTypeOfProperty("innerObj"));
            JSObject innerObj = jsObj.GetPropertyAsJSObject("innerObj");            
            string innerProp1 = innerObj.GetPropertyAsString("prop1");
            Console.WriteLine("innerProp1: " + innerProp1);

            innerObj.SetProperty("prop1", "Update Value");
            Console.WriteLine("innerObj.innerProp1: " + innerObj.GetPropertyAsString("prop1")); // "innerObj.innerProp1: Update Value"

            innerObj.SetProperty("prop3", "Value of Added Property");
            Console.WriteLine("innerObj.innerProp3: " + innerObj.GetPropertyAsString("prop3")); // "innerObj.innerProp3: Value of Added Property"

            JSObjectExample.Log("jsObj: ", jsObj);
            JSObject originalObj = JSHost.GlobalThis.GetPropertyAsJSObject("jsObj");
            JSObjectExample.Log("originalObj: ", originalObj);





        }
    }

}