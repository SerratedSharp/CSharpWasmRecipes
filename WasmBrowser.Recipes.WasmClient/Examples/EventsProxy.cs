using System;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;

namespace WasmBrowser.Recipes.WasmClient.Examples;

// See wwwroot/EventsShim.js for implementation details.
public partial class EventsProxy
{
    [JSImport("EventsShim.SubscribeEventById", "EventsShim")]
    public static partial JSObject SubscriveEventById(string elementId, string eventName, 
        [JSMarshalAs<JSType.Function<JSType.String, JSType.String>>] 
        Action<string, string> listenerFunc);

    [JSImport("EventsShim.UnsubscribeEventById", "EventsShim")]
    public static partial void UnsubscriveEventById(string elementId, string eventName, JSObject listenerHandler);

    [JSImport("EventsShim.TriggerClick", "EventsShim")]
    public static partial void TriggerClick(string elementId);



    [JSImport("EventsShim.GetElementById", "EventsShim")]
    public static partial JSObject GetElementById(string elementId);

    [JSImport("EventsShim.SubscribeEvent", "EventsShim")]
    public static partial JSObject SubscribeEvent(JSObject htmlElement, string eventName, 
        [JSMarshalAs<JSType.Function<JSType.Object>>] 
        Action<JSObject> listenerFunc);

    [JSImport("EventsShim.UnsubscribeEvent", "EventsShim")]
    public static partial void UnsubscribeEvent(JSObject htmlElement, string eventName, JSObject listenerHandler);


}

public static class EventsUsage
{
    public static async Task Run()
    {
        await JSHost.ImportAsync("EventsShim", "https://localhost:7017/EventsShim.js");

        Action<string, string> listenerFunc = (eventName, elementId) =>
            Console.WriteLine($"In C# event listener: Event {eventName} from ID {elementId}");
        
        JSObject listnerHandler1 = EventsProxy.SubscriveEventById("btn1", "click", listenerFunc);
        JSObject listnerHandler2 = EventsProxy.SubscriveEventById("btn2", "click", listenerFunc);
        Console.WriteLine("Subscribed to btn1 & 2.");
        EventsProxy.TriggerClick("btn1");
        EventsProxy.TriggerClick("btn2");

        EventsProxy.UnsubscriveEventById("btn2", "click", listnerHandler2);
        Console.WriteLine("Unsubscribed btn2.");
        EventsProxy.TriggerClick("btn1");
        EventsProxy.TriggerClick("btn2");// Doesn't trigger because unsubscribed
        EventsProxy.UnsubscriveEventById("btn1", "click", listnerHandler1);
        // Pitfall: Using a different handler for unsubscribe will silently fail.
        // EventsProxy.UnsubscriveEventById("btn1", "click", listnerHandler2); 
        
        // With JSObject as event target and event object
        Action<JSObject> listenerFuncForElement = (eventObj) =>
        {
            string eventType = eventObj.GetPropertyAsString("type");
            JSObject target = eventObj.GetPropertyAsJSObject("target");
            Console.WriteLine($"In C# event listener: Event {eventType} from ID {target.GetPropertyAsString("id")}");
        };

        JSObject htmlElement = EventsProxy.GetElementById("btn1");
        JSObject listenerHandler3 = EventsProxy.SubscribeEvent(htmlElement, "click", listenerFuncForElement);
        Console.WriteLine("Subscribed to btn1.");
        EventsProxy.TriggerClick("btn1");
        EventsProxy.UnsubscribeEvent(htmlElement, "click", listenerHandler3);
        Console.WriteLine("Unsubscribed btn1.");
        EventsProxy.TriggerClick("btn1");

    }
    // The example displays the following output in the browser's debug console:
    // Subscribed to btn1 & 2.
    // In C# event listener: Event click from ID btn1
    // In C# event listener: Event click from ID btn2
    // Unsubscribed btn2.
    // In C# event listener: Event click from ID btn1    
    // Subscribed to btn1.
    // In C# event listener: Event click from ID btn1    
    // Unsubscribed btn1.    

}