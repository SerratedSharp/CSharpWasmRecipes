using SerratedSharp.SerratedJQ.Plain;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using System.Threading;
using System.Threading.Tasks;
using Uno.Foundation;
using UnoBootstrap.Recipes.WasmClient.Basic;
using static UnoBootstrap.Recipes.WasmClient.HtmlElementWrapper;


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

            //Not needed for Uno Bootstrap, loaded automatically via Require module inclusion SerratedSharp.SerratedJQ.JSDeclarations.LoadScripts();
            
            await SerratedSharp.SerratedJQ.JSDeclarations.LoadJQuery("https://ajax.googleapis.com/ajax/libs/jquery/3.7.1/jquery.min.js");
            //SerratedSharp.SerratedJQ.JSDeclarations.LoadScripts();// declares javascript proxies needed for JSImport
            await JQueryPlain.Ready(); // Wait for document Ready
            //JQueryPlain.Select("base").Remove();// Remove Uno's <base> element that can break relative URL's if embedded.js hosted remotely

            JQueryPlainObject unoBody = JQueryPlain.Select("[id='uno-body'");
            unoBody.Html("<div style='display:none'></div>");// triggers uno observer that hides the loading bar/splash screen

            // We can await values returned from JS promises:
            string resolvedValue = await Basic.PromisesWUno.AwaitFunctionReturningPromisedString();
            Console.WriteLine(resolvedValue);

            // We can also convert JS callbacks to awaitable promises
            await Basic.PromisesWUno.ConvertCallbackToPromise();

            Basic.PromisesWNet7.DeclareJSFunctionReturningPromisedString();
            var fetchBody = await Basic.PromisesWNet7.FunctionReturningPromisedString("https://cat-fact.herokuapp.com/facts/");
            Console.WriteLine("fetchBody: " + fetchBody.Substring(0, 100) + "...");

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
                globalThis.click = function(elementObj) { return elementObj.click(); }
                globalThis.subscribeEvent = function(elementObj, eventName, listenerFunc) 
                { 
                    //elementObj.addEventListener( eventName, listenerFunc, false ); 
                    //return listenerFunc;// Error: JSObject proxy of ManagedObject proxy is not supported. 

                    // Need to wrap the Managed C# action in JS func
                    let handler = function (e) {
                        listenerFunc(e);
                    }.bind(elementObj);

                    elementObj.addEventListener( eventName, handler, false ); 
                    return handler;// return JSObject reference so it can be used for removeEventListener later
                }
                globalThis.unsubscribeEvent = function(elementObj, eventName, listenerFunc) { 
                    return elementObj.removeEventListener( eventName, listenerFunc, false ); 
                } 
                globalThis.subscribeEventWithParameters = function(elementObj, eventName, listenerFunc) 
                { 
                    let intermediateListener = function(event) { 
                        console.log("tst",event);
                        listenerFunc(event, event.type, event.target);  // decompose some event properties into parameters
                    };
                    return elementObj.addEventListener( eventName, intermediateListener, false );
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

            //DemoBasicEvents(element);

            JSObjectWrapperEventHandler<HtmlElementWrapper, JSObject> test = (HtmlElementWrapper sender, JSObject e) =>
            {
                Console.WriteLine($"[Strongly Typed Event Handler] Event fired through C# event' {e.GetPropertyAsString("type")}'");
                // Log sender and event object
                JSObjectExample.Log(sender.JSObject, e);
            };

            //Action<JSObject> test2 = (JSObject e) =>
            //{
            //    Console.WriteLine($"[Strongly Typed Event Handler] Event fired through C# event' {e.GetPropertyAsString("type")}'");
            //    // Log sender and event object
            //    JSObjectExample.Log(e);
            //};

            HtmlElementWrapper htmlElementWrapper = new HtmlElementWrapper(element);
            //htmlElementWrapper.OnClick += HtmlElementWrapper_OnClick;
            //htmlElementWrapper.OnClick -= HtmlElementWrapper_OnClick;
            //htmlElementWrapper.OnClick += test;
            Console.WriteLine("+= test");
            htmlElementWrapper.OnClick += test;
            EventsInterop.Click(element);// trigger event to test hander
            Console.WriteLine("-= test");
            htmlElementWrapper.OnClick -= test;
            EventsInterop.Click(element);// trigger event again to verify event no longer fired
            Console.WriteLine("+= test");
            htmlElementWrapper.OnClick += test;
            EventsInterop.Click(element);



            //EventsInterop.SubscribeEvent(blahBtn, "click", Test);
            //EventsInterop.UnsubscribeEvent(blahBtn, "click", Test);
            //EventsInterop.SubscribeEvent(blahBtn, "click", Test);
            //EventsInterop.UnsubscribeEvent(blahBtn, "click", Test);
            //EventsInterop.SubscribeEvent(blahBtn, "click", Test);
            //EventsInterop.UnsubscribeEvent(blahBtn, "click", Test);

            //EventsInterop.SubscribeEvent(element, "click", htmlElementWrapper.JSInteropEventListener);
            //EventsInterop.UnsubscribeEvent(element, "click", htmlElementWrapper.JSInteropEventListener);
            //EventsInterop.SubscribeEvent(element, "click", htmlElementWrapper.JSInteropEventListener);

            //EventsInterop.Click(element); // Trigger the click event to test event listeners

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

        private static void Btn_OnClick(JQueryPlainObject sender, dynamic e)
        {
            Console.WriteLine("Using JQuery event");
        }

        public static void Test(JSObject element)
        {
            Console.WriteLine("Test");
        }

        private static void DemoBasicEvents(JSObject element)
        {

            // Using a lambda expression as a listener
            EventsInterop.SubscribeEvent(element, "click", (JSObject eventObj) =>
            {
                Console.WriteLine($"[Inline handler] Event fired with event type via interop property '{eventObj.GetPropertyAsString("type")}'");
                JSObjectExample.Log(eventObj);
            });

            // Using local Action variable as listener
            Action<JSObject> listener = (JSObject eventObj) =>
            {
                Console.WriteLine($"[Local variable handler] Event fired with event type via interop property '{eventObj.GetPropertyAsString("type")}'");
                JSObjectExample.Log(eventObj);
            };
            EventsInterop.SubscribeEvent(element, "click", listener);

            EventsInterop.SubscribeEvent(element, "click", ClickListener); // Using static function as listener

            var instance = new SomeClass();
            EventsInterop.SubscribeEvent(element, "click", instance.InstanceClickListener); // Using instance method as listener

            // use local action variable as listener for subscribeEventWithParameters
            Action<JSObject, string, JSObject> listenerWithParameters = (JSObject eventObj, string eventType, JSObject current) =>
            {
                Console.WriteLine($"[Event Destructured Parameters] eventType:{eventType}");
                JSObjectExample.Log(eventObj, eventType, current);// will show that `current` is a HTMLElement reference
            };
            EventsInterop.SusbcribeEventWithParameters(element, "click", listenerWithParameters);
        }

        //private static void HtmlElementWrapper_OnClick(HtmlElementWrapper sender, JSObject e)
        //{
        //    Console.WriteLine($"[Strongly Typed Event Handler] Event fired through C# event' {e.GetPropertyAsString("type")}'");
        //    // Log sender and event object
        //    JSObjectExample.Log(sender.JSObject, e);
        //}

        // SubscribeEvent click listener function
        private static void ClickListener(JSObject eventObj)
        {
            Console.WriteLine($"[ClickListener handler] Event fired with event type via interop property '{eventObj.GetPropertyAsString("type")}'");
            JSObjectExample.Log(eventObj);
        }
    }

    public class SomeClass
    {
        // SubscribeEvent click listener function
        public void InstanceClickListener(JSObject eventObj)
        {
            Console.WriteLine($"[ClickListener handler] Event fired with event type via interop property '{eventObj.GetPropertyAsString("type")}'");
            JSObjectExample.Log(eventObj);
        }
    }

    public interface IJSObjectWrapper { JSObject JSObject { get; } }
    public class HtmlElementWrapper(JSObject jsObject) : IJSObjectWrapper
    {
        public JSObject JSObject { get; } = jsObject;
        
        // SubscribeEvent click listener function
        public void InstanceClickListener(JSObject eventObj)
        {
            Console.WriteLine($"[ClickListener handler] Event fired with event type via interop property '{eventObj.GetPropertyAsString("type")}'");
            JSObjectExample.Log(eventObj);
        }

        public delegate void JSObjectWrapperEventHandler<in TSender, in TEventArgs>(TSender sender, TEventArgs e)
            where TSender : IJSObjectWrapper;
        private JSObjectWrapperEventHandler<HtmlElementWrapper, JSObject> onClick;
        const string EventName = "click";
        private JSObject JSListener { get; set; }
        public event JSObjectWrapperEventHandler<HtmlElementWrapper, JSObject> OnClick
        {
            add
            {
                // We have only one JS listener, and can proxy firings to all C# subscribers
                if (onClick == null) // if first subscriber, then add JS listener
                { 
                    JSListener = EventsInterop.SubscribeEvent(this.JSObject, EventName, JSInteropEventListener);
                }
                // Always add the C# subscriber to our event collection
                onClick += value;
            }
            remove
            {
                if (onClick == null)// if no subscribers on this instance/event
                {
                    return;// nothing to remove
                }

                onClick -= value; // else remove susbcriber
                
                if (onClick == null) // if last subscriber removed, then remove JS listener
                {
                    EventsInterop.UnsubscribeEvent(this.JSObject, EventName, JSListener);
                }
            }
        }
        // When the JS event is fired, this listener is called, and fires the C# event
        public void JSInteropEventListener(JSObject eventObj)
        {
            onClick?.Invoke(this, eventObj);// fire event on all subscribers
        }
    }

}