using System;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;

namespace WasmBrowser.Recipes.WasmClient.Examples.StronglyTypedEvents;



// This example builds upon EventsProxy.cs and EventsShims.js
public interface IJSObjectWrapper { JSObject JSObject { get; } }
public class Element(JSObject jsObject) : IJSObjectWrapper
{
    public JSObject JSObject { get; } = jsObject;

    // SubscribeEvent click listener function
    public void InstanceClickListener(JSObject eventObj)
    {
        Console.WriteLine($"[ClickListener handler] Event fired with event type via interop property '{eventObj.GetPropertyAsString("type")}'");
        PrimitivesProxy.ConsoleLog(eventObj);
    }

    public delegate void JSObjectWrapperEventHandler<in TSender, in TEventArgs>(TSender sender, TEventArgs e)
        where TSender : IJSObjectWrapper;
    private JSObjectWrapperEventHandler<Element, JSObject> onClick;
    const string EventName = "click";
    private JSObject JSListener { get; set; }
    public event JSObjectWrapperEventHandler<Element, JSObject> OnClick
    {
        add
        {
            // We have only one JS listener, and can proxy firings to all C# subscribers
            if (onClick == null) // if first subscriber, then add JS listener
            {
                JSListener = EventsProxy.SubscribeEvent(this.JSObject, EventName, JSInteropEventListener);
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
                EventsProxy.UnsubscribeEvent(this.JSObject, EventName, JSListener);
            }
        }
    }
    // When the JS event is fired, this listener is called, and fires the C# event
    private void JSInteropEventListener(JSObject eventObj)
    {
        onClick?.Invoke(this, eventObj);// fire event on all subscribers
    }
}

public partial class Document
{
    public static Element GetElementById(string id)
    {
        JSObject jsObject = StronglyTypedWrapper.DocumentProxy.GetElementById(id);
        return new Element(jsObject);// Wrap the JSObject in our Element wrapper.
    }
}


public static class StronglyTypedEventsUsage
{
    public static async Task Run()
    {
        await JSHost.ImportAsync(StronglyTypedWrapper.DomShimModule.ModuleName, $"/{StronglyTypedWrapper.DomShimModule.ModuleName}.js");

        var element = Document.GetElementById("btn3");

        Element.JSObjectWrapperEventHandler<Element, JSObject> test = 
            (Element sender, JSObject e) => {
                Console.WriteLine($"[Strongly Typed Event Handler] Event fired through C# event '{e.GetPropertyAsString("type")}'");
                // Log sender and event object
                PrimitivesProxy.ConsoleLog(sender.JSObject);
            };

        Console.WriteLine("+= test");
        element.OnClick += test;
        EventsProxy.TriggerClick("btn3");// trigger event to test hander
        Console.WriteLine("-= test");
        element.OnClick -= test;
        EventsProxy.TriggerClick("btn3");// trigger event again to verify event no longer fired
        Console.WriteLine("+= test");
        element.OnClick += test;
        EventsProxy.TriggerClick("btn3");
    }
}
// The example displays a red "New Hello!" element in the browser.



