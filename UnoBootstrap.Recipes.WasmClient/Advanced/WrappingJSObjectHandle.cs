using Uno.Foundation.Interop;
using Uno.Foundation;

namespace UnoBootstrap.Recipes.WasmClient.Advanced
{
    public class ElementWrapper : IJSObject
    {
        internal JSObjectHandle handle;
        JSObjectHandle IJSObject.Handle { get => handle; }
        // internal constructor, objects are created/returned from static methods
        internal ElementWrapper()
        {
            handle = JSObjectHandle.Create(this);
        }
        // static method returning a new instance referencing a JS object
        public static ElementWrapper GetElementById(string id)
        {
            var elementWrapper = new ElementWrapper();// Create a new empty wrapper
            // From JS, assign the javascript object to a property of our wrapper
            WebAssemblyRuntime.InvokeJSWithInterop(
                $"{elementWrapper}.obj = document.getElementById('{id}');");
            return elementWrapper;
        }

        public string GetClass() // Instance method on the wrapped type
        {
            // passing in `this`, an instance of IJSObject and accessing 
            // the previously assigned property holding the JS element object
            // and calling an instance method of the contained object
            return WebAssemblyRuntime.InvokeJSWithInterop(
                $"return {this}.obj.getAttribute('class');");
        }
    }
}


