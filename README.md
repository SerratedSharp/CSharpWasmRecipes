# SerratedWasmRecipes
Collection of code snippets and utilities for C# WASM, primarily focused on browser hosted WASM using Uno.Wasm.Bootstrap.

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

## JS Interop Scenarios

This section deals primarily with exposing types or methods from JS to C#.  There are a multitude of options for interop between C# and JS types.  The history of feature gaps in WASM spec and implementations has led to varied approaches of filling the gaps.  Many examples and tutorials online are specific to Uno Platform or Blazor, and may not work in Uno.Bootstrap.Wasm.  Here we focus on solutions that work with Uno.Bootstrap.Wasm in the absence of Blazor or Uno Platform UI frameworks, to eliminate some of the guess work and helping devs identify a solution appropriate to their needs.

### Static Methods

#### Using .NET 7

Expose static JS methods to C#:
```C#
    public partial class JSGlobal
    {
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

### Instances and Instance Methods

#### Using .NET 7

Declaring our own static JS methods (either included in a loaded *.js resource, or declared with InvokeJS):
```js
// findElement takes a string and returns an object (an HTML element reference)
// getClass takes an object, calls an instance method on that object, and returns a string
globalThis.findElement = function(id) { return document.getElementById(id); }
globalThis.getClass = function(obj) { return obj.getAttribute('class'); }
```

Expose static JS methods to C#:
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

This opens up scenarios for creating instance wrappers, where a C# type can hold a reference to a JSobject it wraps.

## Upcoming

Additional examples are available in the Basic/Intermediate source code folders, and these examples will be expanded within the coming weeks.





