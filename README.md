# SerratedWasmRecipes
Collection of code snippets, utilities, and guidance for C# WASM, primarily focused on browser hosted WASM using Uno.Wasm.Bootstrap.

There are a multitude of options for interop between C# and JS types.  The history of feature gaps in WASM spec and implementations has led to varied approaches of filling the gaps.  Many examples and tutorials online are specific to Uno Platform or Blazor, and may or may not work in Uno.Bootstrap.Wasm.  Here we focus on solutions that work with Uno.Bootstrap.Wasm in the absence of Blazor or Uno Platform UI frameworks, to eliminate some of the guess work and helping devs identify a solution appropriate to their needs.

## Interop Supporting Runtimes

It is helpful to be familiar with the ecosystem of different libraries and frameworks that provide JS interop capability within .NET, so devs can recognize what doucmentation or tools are applicable to their environment.

- Microsoft.JSInterop.IJSRuntime/JSRuntime: Typically used by code specific to the Blazor framework.  *To my knowledge* this cannot be used in Uno.Bootstrap.Wasm without hooks into the Host initilization that would typically instantiate and register the JSRuntime into the DI ServiceCollection.
- Uno.Foundation.WebAssemblyRuntime/Uno.Foundation.Interop: This provides the majority of interop capabilities for Uno.Bootstrap.Wasm prior to first class support for interop in .NET. The nuget package Uno.Foundation.Runtime.WebAssembly provides these capabilities, with the namespace Uno.Foundation.Interop containing most of the relevant types.
- .NET 7, System.Runtime.InteropServices.JavaScript: Many JS interopt capabilities were implemented in .NET 6 and expanded in .NET 7.  These are compatible with Uno.Bootstrap.Wasm, and are typically more performant, although sometimes more restrictive.  We'll refer to examples leveraging this capability as simply ".NET 7".
- Uno.Bootstrap.Wasm: This handles compiling and packaging our .NET assembly as a WebAssembly/WASM, along with all the javascript necessary for loading(i.e. **bootstrap**ping) the WASM into the browser.  This is only intended for use in the root project which will be the entry point for the WebAssembly.  Other class libraries/projects referenced do not need to reference this package.   Note, no relation to the Bootstrap CSS/JS frontend framework.
- Uno.UI.WebAssembly: This package contains some javascript declarations what WebAssemblyRuntime is dependent on. For example, `WebAssemblyRuntime.InvokeAsync()` will fail at runtime if this package has not been included.  This package only needs to be referenced from the root project which also references Uno.Bootstrap.Wasm.

> [!IMPORTANT] 
> There are some type names that exist in both WebAssemblyRuntime and System.Runtime.InteropService.Javascript, such as JSObject.  Be mindful of what namespaces you have declared in `using`, or fully qualify, to avoid confusing compilation errors.  A project can leverage both capabilities in different places, but would not mix them for a given C#/JS function/type mapping.

 The WebAssemblyRuntime package or .NET 7 InteropServices namespace can also be used from class library projects implementing interop wrappers which are intended for consumption in a Uno.Boostrap.Wasm project.

## JS Interop Scenarios
This section deals primarily with exposing types or methods from JS to C#, with the intention of allowing C# code to call into JS, or hold references to JS objects.

### Importing Static JS Methods

#### Using .NET 7

Expose static JS methods to C#:
```C#
public partial class JSGlobal
{
    // mapping existing console.log to a C# method
    [JSImport("globalThis.console.log")]
    public static partial void GlobalThisConsoleLog(string text);        
}
    
//... calling from WebAssembly Program.cs, we'd expect output to appear in the browser console
JSGlobal.GlobalThisConsoleLog("Hello World");
```

Declaring our own static JS method(either included in a loaded *.js resource, or declared with InvokeJS):
```js
globalThis.alertProxy = function (text) {
	alert(text);
}
```

Mapping it to a C# method:
```C# 
public partial class JSGlobalProxy
{
	[JSImport("globalThis.alertProxy")]
	public static partial void AlertProxy(string text);
}
//... called from WASM Program.cs
JSGlobalProxy.AlertProxy("Hello World");
```

#### Using WebAssemblyRuntime

Expose static JS methods to C#:
```C#
public static void ConsoleLog(string message)
{
    WebAssemblyRuntime.InvokeJS($"""console.log("{message}");""");           
}
```

Declaring our own static JS method(either included in a loaded *.js resource, or declared with InvokeJS):
```js
globalThis.alertProxy = function (text) {
	alert(text);
}
```

Mapping it to a C# method:
```C# 
public static class JSGlobalProxy
{
    public static void AlertProxy(string message)
    {
	    WebAssemblyRuntime.InvokeJS($"""globalThis.alertProxy("{message}");"""); 
    }
}
//... called from WASM Program.cs
JSGlobalProxy.AlertProxy("Hello World");
```

### Instances and Instance Methods

WebAssemblyRuntime and .NET 7 each offer types that represent a javascript object reference.  Although they are not strongly typed and expose limited functionality, the ability to hold a reference to it and return/pass it across the interop layer opens up a multitude of capabilities.  For example, the SerratedJQ .NET JQuery wrapper is able to simulate a strongly typed JS implementation using only a non-specific internal JS object reference.

#### Using .NET 7 JSObject

The System.Runtime.InteropService.Javascript.JSObject type can be used in function signatures as parameters or return types in `[JSImport]`/`[JSExport]` attributed methods and represents a reference to a javascript object.  

Declaring static javascript methods:
```js
// findElement takes a string and returns an object (an HTML element reference)
// getClass takes an object, calls an instance method on that object, and returns a string
globalThis.findElement = function(id) { return document.getElementById(id); }
globalThis.getClass = function(obj) { return obj.getAttribute('class'); }
```

Import static JS methods into C#:
```C#
using System.Runtime.InteropServices.JavaScript;
public partial class JSObjectExample
{
    [JSImport("globalThis.findElement")]
    public static partial JSObject FindElement(string id);

    [JSImport("globalThis.getClass")]
    public static partial string GetClass(JSObject obj);
```

Then call the methods through the JSObjectExample C# proxy, passing the JSObject reference into it so it can call an instance method on it:
```C#
// Call the static JS functions from C#
JSObject element = JSObjectExample.FindElement("uno-body");
// Pass the handle to another method
var elementClasses = JSObjectExample.GetClass(element);
Console.WriteLine("Class string: " + elementClasses);
```

Now any static method can implement instance semantics by taking a JSObject as its first parameter, and in turn calling the instance method on the JSObject.  This could be evolved into a wrapper class that holds the JSObject as an internal field and exposes instance methods to present a more netural interface.

#### Using WebAssemblyRuntime IJSObject and InvokeJSWithInterop

Whenever an instance implementing IJSObject is created and we call JSOjbectHandle.Create(), a javascript object is created and tracked.  If an instance of IJSObject is passed directly into InvokeJSWithInterop via string interpolation, the corresponding javascript object is used in its place.  We choose an arbitrarily named property to hold and access the javascript object we are wrapping.

```C#
using Uno.Foundation.Interop;
using Uno.Foundation;
public class ElementWrapper : IJSObject
{
    internal JSObjectHandle handle;    
    JSObjectHandle IJSObject.Handle { get => handle; }
    // internal constructor, objects are created/returned from static methods
    internal ElementWrapper() {
        handle = JSObjectHandle.Create(this);
    }
    // static method returning a new instance referencing a JS object
    public static ElementWrapper GetElementById(string id)
    {
        var elementWrapper = new ElementWrapper();// Create a new empty wrapper
        // From JS, assign the javascript object to a property of our wrapper
        WebAssemblyRuntime.InvokeJSWithInterop(
            $"{elementWrapper}.obj = document.getElementById('{id}')");
        return elementWrapper;
    }

    public string GetClass() // Instance method on the wrapped type
    {            
        // passing in `this`, an instance of IJSObject and accessing 
        // the previously assigned property holding the JS element object
        // and calling an instance method of the contained object
        return WebAssemblyRuntime.InvokeJSWithInterop(
            $"{this}.obj.getAttribute('class')");
    }
```


## Promises.cs
Demonstrates approaches to exposing a JS promise, async method, or old style callback as an async method in C# that can be awaited.  Includes returning a string from JS to C#.  Demonstrates awaiting RequireJS dependency resolution where C# code needs to wait for a JS dependency to load.

## Security.cs
Demonstrates the risks of unencoded strings, and how to properly encode and fence string parameters passed from C# to JS to avoid XSS attacks that would attempt to abuse `InvokeJS`.

In general, the following guidelines should be followed:
 - User generated strings should only be embedded as parameters.
 - User generated strings passed as string parameters in JS must be escaped with EscapeJs.
 - User generated strings must be fenced in unescaped double quotes
 -- (the order you apply the escaping and then wrap in quotes is important)        
        
Note: As with any consideration for XSS, the phrase "user generated strings" covers a wide variety of scenarios where a string potentially originated from a user or other untrusted source.  For example, in a given context, you may have queried a string from a trusted API or trusted database, but the string may be untrusted because it was at some point submitted and saved from user input.  Consider where user's provide a description of themselves, and that description is stored in the DB, and can be viewed by other users.  A variety of attacks could be embedded in this string, such as HTML injection or JS injection, but could also contain legitimately benign characters that aren't malicious in nature, yet could interfere in a context, and yet should also be rendered correctly.  No sanitization is universal in protecting from all of these attacks without potentially mangling the string.  You must apply the appropriate encoding based on the context where the content is being embedded to ensure it is both secure and rendered correctly.  You may need to apply multiple encodings in a specific order based on nested contexts.  See `Security.SafeInvokeJSWithHtml()`

 Note: Use of JSImport rather than InvokeJS can eliminate the need for JS encoding parameters.  It does not obsolve you of addressing other encoding contexts.


## Upcoming

Additional examples are available in the Basic/Intermediate source code folders, and these examples will be expanded within the coming weeks.





