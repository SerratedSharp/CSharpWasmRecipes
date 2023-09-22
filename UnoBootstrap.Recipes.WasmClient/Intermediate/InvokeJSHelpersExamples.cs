using Newtonsoft.Json;
using System;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.JavaScript;
using Uno.Foundation;
using JS = UnoBootstrap.Recipes.WasmClient.InvokeJSHelpers;


namespace UnoBootstrap.Recipes.WasmClient.JSWrappers
{
    // Demonstrates pattersn for calling existing javascript methods.

    // Proxies for document such as document.getElementById()
    public static class Document
    {
        private const string baseObject = nameof(Document);

        // C#: Document.CreateTextNode("Some Text"); rendered in JS as: document.createTextNode('Some Text');
        // C#: Document.CreateElement("div"); rendered in JS as: document.createElement('div');
        // C#: Document.GetElementById("someId"); rendered in JS as: document.getElementById('someId');`

        // Because CallJSOfSameName uses [CallerMemberName], we can easily create proxy calls for multiple functions by naming the caller the same as the JS function
        public static void CreateTextNode(string text)          => InvokeJSHelpers.CallJSOfSameName(baseObject, text);        
        public static void CreateElement(string elementName)    => InvokeJSHelpers.CallJSOfSameName(baseObject, elementName);
        public static void GetElementById(string id)            => InvokeJSHelpers.CallJSOfSameName(baseObject, id);

        // JS Functions can return strings/primitive values to C#.
        // In this case, we will get "true" or "false" as a string:
        public static string HasFocus() => InvokeJSHelpers.CallJSOfSameName(baseObject);

        // Alternatively we can parse the string into a bool in C#:
        public static bool HasFocusWithType() => 
            bool.Parse( InvokeJSHelpers.CallJSOfSameName(baseObject, funcName: "HasFocus"));

        // More robust parsing would use a javascript knowledgeable parser:
        public static bool HasFocusWithJSParsedType() => 
            JsonConvert.DeserializeObject<bool>(InvokeJSHelpers.CallJSOfSameName(baseObject, funcName: "HasFocus"));

        // Remember to use nullable types where null results are possible
        public static bool? HasFocusWithJSParsedNullableType() =>
            JsonConvert.DeserializeObject<bool?>(InvokeJSHelpers.CallJSOfSameName(baseObject, funcName: "HasFocus"));


        // TODO: Implement JS helpers for async, using document.exitFullscreen() as example that returns a promise
    }

    // Proxies for JS console.log()
    public static class Console 
    {
        // Object name will be rendered in JS with the first letter lowered cased, e.g. a C# class named JQuery becomes JS "jQuery"
        private const string objectName = nameof(Console);        
        // Alternatively if we don't want to be coupled to the class name, jsut declare `private static const string objectName = "console";`

        // Note: we aliased InvokeJSHelpers to "JS." in the using declarations at the top to make code more concise.

        // Demonstrates various parameter patterns/overloads.
        public static void Log(string message) => JS.CallJSOfSameName(objectName, message); //  console.log('message');
                                                                                            
        public static void Log(params object[] itemsToWrite) => JS.CallJSOfSameName(objectName, itemsToWrite);
        // C#: Log("Some Values: ", 1, false, DateTime.Now);
        // Renders as JS: console.log('Some Values: ', '1', false, '30/12/2023 07:53:53');
        public static void Log(object value, DateTime? now = null)
            => JS.CallJSOfSameName(objectName, $"Value {value} of {value.GetType().Name} at {now ?? DateTime.Now}");

        public static void Log(string message, object value, DateTime now)
            => JS.CallJSOfSameName(objectName, message, value, now);

        // ToParams is shorthand for new object[]{}, necesary we have more than 4 params.  Since CallJSOfSameName leveraged optional params, it cannot also use the params keyword.
        public static void Log(string messageBefore, object valueBefore, DateTime timeBefore, string messageAfter, object valueAfter, DateTime timeAfter)
            => JS.CallJSOfSameName(objectName, JS.ToParams(messageBefore, valueBefore, timeBefore, messageAfter, valueAfter, timeAfter));

    }
    
    // We don't have to follow the class naming convention, and just use a constant to define the base object name.
    public static class JavascriptConsole
    {
        private const string objectName = "console";
        public static void Log(params object[] itemsToWrite) => JS.CallJSOfSameName(objectName, itemsToWrite);
    }

}
