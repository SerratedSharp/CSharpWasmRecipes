using System;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;

namespace WasmBrowser.Recipes.WasmClient.Examples;

// See wwwroot/JSObjectShim.js for implementation details.
public partial class JSObjectProxy
{
    [JSImport("JSObjectShim.CreateObject", "JSObjectShim")]
    public static partial JSObject CreateObject();

    [JSImport("JSObjectShim.IncrementAnswer", "JSObjectShim")]
    public static partial void IncrementAnswer(JSObject jsObject);

    [JSImport("JSObjectShim.Summarize", "JSObjectShim")]
    public static partial string Summarize(JSObject jsObject);

    [JSImport("globalThis.console.log")]
    public static partial void ConsoleLog([JSMarshalAs<JSType.Any>] object value);

}

public static class JSObjectUsage
{
    public static async Task Run()
    {
        await JSHost.ImportAsync("JSObjectShim", "/JSObjectShim.js");

        JSObject jsObject = JSObjectProxy.CreateObject();
        JSObjectProxy.ConsoleLog(jsObject);
        JSObjectProxy.IncrementAnswer(jsObject);
        // Note: We did not retrieve an updated object, and will see the change reflected in our existing instance.
        JSObjectProxy.ConsoleLog(jsObject);

        // JSObject exposes several methods for interacting with properties:
        jsObject.SetProperty("question", "What is the answer?");
        JSObjectProxy.ConsoleLog(jsObject);

        // We can't directly JSImport an instance method on the jsObject, but we can
        // pass the object reference and have the JS shim call the instance method.
        string summary = JSObjectProxy.Summarize(jsObject);
        Console.WriteLine("Summary: " + summary);

    }
}
// The example displays the following output in the browser's debug console:
//     {name: 'Example JS Object', answer: 41, question: null, Symbol(wasm cs_owned_js_handle): 5, summarize: ƒ}
//     {name: 'Example JS Object', answer: 42, question: null, Symbol(wasm cs_owned_js_handle): 5, summarize: ƒ}
//     {name: 'Example JS Object', answer: 42, question: 'What is the answer?', Symbol(wasm cs_owned_js_handle): 5, summarize: ƒ}
//     Summary: The question is "What is the answer?" and the answer is 42.