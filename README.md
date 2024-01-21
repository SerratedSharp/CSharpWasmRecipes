Access the table of contents via the ![image](https://github.com/SerratedSharp/CSharpWasmRecipes/assets/97156524/49113928-1bd7-4e7c-8c28-b1aafa035744) icon.

# C# WASM Recipes
Collection of code snippets, utilities, and guidance for C# WASM, primarily focused on browser hosted WASM using Uno.Wasm.Bootstrap.  The focus being on non-Blazor and non-UnoPlatform applications, as there is already a wealth of examples and documentation for these, while less so for platform agnostic .NET/C# WASM.  There are a multitude of options for interop between C# and JS types.  The history of feature gaps in WASM spec, rapid evolution, and implementations has led to varied approaches.

We focus on platform agnostic solutions with Uno.Bootstrap.Wasm in the absence of Blazor or Uno Platform UI frameworks.  Many of the examples, tutorials, and documentation online are specific to Uno Platform or Blazor, and may not work in Uno.Bootstrap.Wasm. This documentation focuses on  platform agnostic approaches to eliminate some of the guess work and help devs identify a solution appropriate to their needs.

I am a full stack developer with nearly 20 years of .NET experience, and have been working with WASM for about 2 years.  The following represents approaches I've developed and believe to be effective, however I am not an expert in WebAssembly.  Official documentation may differ, but I created this resource because I've found official documentation to sometimes be out of date, incomplete, or biased towards a platform specific approach.

## Interop Supporting Runtimes

It is helpful to be familiar with the ecosystem of different libraries and frameworks that provide JS interop capability within .NET, so devs can recognize what documentation or tools are applicable to their environment.

- .**NET 7, System.Runtime.InteropServices.JavaScript**: .NET 6 included many JS interop capabilities and were expanded in .NET 7.  We'll refer to examples leveraging this capability as simply ".NET 7".  These are typically compatible with both Uno.Bootstrap.Wasm or Blazor.  In some ways it is more restrictive than prior Uno WebAssemblyRuntime capabilities because you cannot generate and execute arbitrary javascript on the fly nor interpolate javascript in C#, and requires more boilerplate code to accomplish similar tasks such as requiring two extra layers of instance method proxies for both the C# and JS layer (however techniques can mitigate the proliferation of these proxy methods).  Despite that, .NET 7's interop is a significant improvement:
  - Prior approaches required JS objects or managed types to be explicitly marshaled which varied from easy to impossible in difficulty depending on the scenario.  Parameter fencing had to be carefully guarded for security, and managed types had to sometimes be manually pinned/unpinned.  This is a short list that doesn't fully explore the complexities of implementing interop prior to .NET 6.
  - .NET 6/7 has native support for JS and managed object handles on both sides of the interop layer, ensures references across the interop boundary are not garbage collected prematurely, and ensures function parameters cannot break out of quotes/parameter contect.  Exporting and importing methods is more straightforward and passing references originating from JS is simplified.  **Overall using .NET 7's \*.InteropServices.JavaScript is the preferred approach.**
- **Uno.Foundation.WebAssemblyRuntime/Uno.Foundation.Interop**: This provides the majority of legacy interop capabilities for Uno.Bootstrap.Wasm prior to first class support for interop in .NET. The Nuget package Uno.Foundation.Runtime.WebAssembly provides these capabilities, with the namespace Uno.Foundation.Interop containing most of the relevant types.  It is recommended to use .NET 7 capabilities instead where possible.  Additionally, some capabilities such as InvokeJSWithInterop were only intended for internal Uno use, and within Uno's codebase migration to .NET 7 approaches can be observed.  As such, most of my snippets for these approaches will be archived.
- **Microsoft.JSInterop.IJSRuntime/JSRuntime**: Typically used by code specific to the Blazor framework.  *To my knowledge* this cannot be used in non-Blazor contexts such as Uno.Bootstrap.Wasm without hooks into the Host initialization that would typically instantiate and register the JSRuntime into the DI ServiceCollection.

Additional Packages:
- **Uno.Bootstrap.Wasm**: Tooling for compiling and packaging our .NET assembly as a WebAssembly/WASM package, along with all the javascript necessary for loading(i.e. **bootstrap**ping) the WASM into the browser.  This is only intended for use in the root project which will be the entry point for the WebAssembly.  Other class libraries/projects referenced do not need to reference this package.   (Note, no relation to the Bootstrap CSS/JS frontend framework.)  The name "Bootstrap" refers to similar terminology used for loading operating systems, as it "pulls itself up by its bootstraps".
- **Uno.Wasm.Bootstrap.DevServer**: Provides a self-hosted HTTP server for serving static WASM files locally and supporting debugging browser link to enable breakpoints and stepping through C# code in the IDE while it is running as WASM inside the browser.  This package is useful during local development, but would likely be eliminated when hosted in test/production, where you would likely package the WASM package and related javascript files to be served statically from a traditional web server.  For example, I would include the WASM package as a subfolder of my ASP.NET MVC project's wwwroot to be served as static files.
- **Uno.UI.WebAssembly**: At one time this package generated some javascript declarations that WebAssemblyRuntime was dependent on. For example, `WebAssemblyRuntime.InvokeAsync()` would fail at runtime if this package had not been included. At least since 8.* release, this package is no longer required for vanilla WASM projects.
- **SerratedSharp.JSInteropHelpers**: An optional library of helper methods for implementing interop. Reduces the amount of boilerplate code needed to call JS methods (both static and instance) from C#.  This library is less refined as it is primarily used internally to support my own JS interop implementations, but it has been key in allowing me to implement large surface areas of JS library APIs quickly.  I hope to refine this library for other JS interop/wrapper implementers to use in the future.  Usage examples can be found within SerratedJQ implementation: [JQueryPlainObject.cs](https://github.com/SerratedSharp/SerratedJQ/blob/main/SerratedJQLibrary/SerratedJQ/Plain/JQueryPlainObject.cs)
- **SerratedSharp.SerratedJQ**: An optional .NET wrapper for jQuery to enable expressive access and event handling of the HTML DOM from WebAssembly.

> [!IMPORTANT] 
> There are some type names that exist in both WebAssemblyRuntime and System.Runtime.InteropService.Javascript, such as JSObject.  Be mindful of what namespaces you have declared in `using`, or fully qualify, to avoid confusing compilation errors.  A project can leverage both capabilities in different places, but should not mix them for a given C#/JS function/type mapping.

 The WebAssemblyRuntime package or .NET 7 InteropServices namespace can also be used from class library projects implementing interop wrappers which are intended for consumption in a Uno.Boostrap.Wasm project.  The library would typically **not** reference Uno.Bootstrap.Wasm, as that's only needed for the root Console project with a `Main` entry point, and thus would be compiled into a WebAssembly package.

## Architecture and Debugging

The [Architecture](Architecture.md) overview covers the structure of a new or existing website integrating a WebAssembly package, possible structures of Projects/Solution, and an overview of enabling debugging.  [SerratedJQSample](https://github.com/SerratedSharp/SerratedJQ/tree/main/SerratedJQSample) includes project configuration for debugging in VS2022, and debugging setup is included in the SerratedJQ [Quick Start Guide](https://github.com/SerratedSharp/SerratedJQ#quick-start-guide).  SerratedJQ is not a requirement (its purpose is to provide DOM access), and following the Quick Start guide while ommitting SerratedJQ will result in a basic project setup for an ASP.NET MVC web application which includes a WebAssembly package. 

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

Declaring our own static JS method:
```js
globalThis.alertProxy = function (text) {
	alert(text);
}
```

The above is what we would informally call the JS proxy or JS shim.  In this case it shims or fills the gap between our .NET implementation and existing JS capabilities/libraries.  JS declarations such as this would be loaded from either a *.js resource or declared with InvokeJS.  Some opt to implement shims in typescript, but you will be need to be knowledgeable of the full javascript typename to ensure correct name is referenced from JSImport.

Mapping it to a C# method proxy:
```C# 
public partial class GlobalProxyJS
{
	[JSImport("globalThis.alertProxy")]
	public static partial void AlertProxy(string text);
}
//... called from WASM Program.cs
GlobalProxyJS.AlertProxy("Hello World");
```

#### Loading JS Declarations

Static JS declarations such as JS shims can be loaded from files using traditional `<script src='*'>` references before the `embedded.js` inclusion, via requireJS which is initialized by Uno Bootstrap scripts, or by executing the declaration from a string using WebAssemblyRuntime.InvokeJS.  

The WasmClient project can include JS declarations as embedded files, and then execute them from Program.Main() of the WasmClient project using WebAssemblyRuntime.InvokeJS():

```C#
WebAssemblyRuntime.InvokeJS(YourAssemblyWasmClient.EmbeddedFiles.JSShimsFile);
```

Additional methods of including JS per Uno Bootstrap documentation:
[Loading via Uno.Bootstrap and RequireJS] (https://platform.uno/docs/articles/external/uno.wasm.bootstrap/doc/features-dependency-management.html)
[Embedding Existing JavaScript Components Into Uno-WASM](https://platform.uno/docs/articles/interop/wasm-javascript-1.html#embedding-assets)

### JS Object References

#### Using .NET 7 JSObject

The System.Runtime.InteropServices.JavaScript.JSObject type can be used in function signatures as parameters or return types in `[JSImport]`/`[JSExport]` attributed methods and represents a reference to a javascript object.  (Warning: Be mindful of using references as Uno.Foundation.Interop contains a different legacy JSObject type that will not work in the below examples.)

```C#
using System.Runtime.InteropService.JavaScript;
public partial class JSObjectExample
{    
        [JSImport("globalThis.JSON.parse")]
        public static partial JSObject GetJsonAsJSObject(string jsonString);

        [JSImport("globalThis.console.log")]
        public static partial void ConsoleLogJSObject(JSObject obj);
}

//Usage:
JSObject jsObj = JSObjectExample.GetJsonAsJSObject("""{"firstName":"Crow","middleName":"T","lastName":"Robot"}""");
JSObjectExample.ConsoleLogJSObject(jsObj);
```

The GetJsonAsJSObject method takes a string, then deserializes it to an JS object, and returns the JSObject reference.

Browser Console Output:

![image](https://github.com/SerratedSharp/CSharpWasmRecipes/assets/97156524/317a6793-2783-4ddd-a5ce-0df12acc5f1a)

#### Accessing JSObject Properties

Just about any property or method of a JSObject can be accessed by declaring a JSProxy and implementing custom JS:

```JS
// Concatenate first, middle, and last name:
globalThis.concatenateName = function (nameObject) {
	return obj.firstName + " " + obj.middleName + " " + obj.lastName;
}
```

```C#
public partial class JSObjectExample
{
    [JSImport("globalThis.concatenateName")]
    public static partial string ConcatenateName(JSObject nameObject);
}
// Usage:
JSObject jsObj = JSObjectExample.GetJsonAsJSObject("""{"firstName":"Crow","middleName":"T","lastName":"Robot"}""");
string fullName = JSObjectExample.ConcatenateName(jsObj);
```

The above may be appropriate where multiple operations can occur in a single JS interop call.  Alternatively, the JSObject exposes a series of methods for accessing or setting properties of the underlying type:

![image](https://github.com/SerratedSharp/CSharpWasmRecipes/assets/97156524/e24684e6-12be-4ab0-b972-f6e7a47d6bcb)

```C#
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
    
// Store a reference in JS globalThis to demonstrate by ref modification
JSHost.GlobalThis.SetProperty("jsObj", jsObj); 

JSObjectExample.ConsoleLogJSObject(jsObj);
string lastName = jsObj.GetPropertyAsString("lastName");
Console.WriteLine("LastName: " + lastName); // "LastName: Robot"
Console.WriteLine("Type: " + jsObj.GetType()); // "Type: System.Runtime.InteropServices.JavaScript.JSObject"
Console.WriteLine("lastName Type: " + jsObj.GetTypeOfProperty("lastName")); // "lastName Type: string"

Console.WriteLine("innerObj Type: " + jsObj.GetTypeOfProperty("innerObj")); // "innerObj Type: object"
JSObject innerObj = jsObj.GetPropertyAsJSObject("innerObj");            
string innerProp1 = innerObj.GetPropertyAsString("prop1"); 
Console.WriteLine("innerProp1: " + innerProp1); // "innerProp1: innerObj Prop1 Value"

innerObj.SetProperty("prop1", "Update Value");
Console.WriteLine("innerObj.innerProp1: " + innerObj.GetPropertyAsString("prop1")); // "innerObj.innerProp1: Update Value"

innerObj.SetProperty("prop3", "Value of Added Property"); // Add new property
Console.WriteLine("innerObj.innerProp3: " + innerObj.GetPropertyAsString("prop3")); // "innerObj.innerProp3: Value of Added Property"
```

Modifying or adding properties in this way via the JSObject reference will also affect the original JS object if there were references to it from JavaScript as we can see with the object reference we assigned to globalThis:
```C#
JSObjectExample.Log("jsObj: ", jsObj);
JSObject originalObj = JSHost.GlobalThis.GetPropertyAsJSObject("jsObj");
JSObjectExample.Log("originalObj: ", originalObj);
```

Comparing the reference we modified throughout the code with the original reference we stored and retrieved from JS globalThis, we see the new and modified properties are reflected in the original JS object we stored in globalThis:

![image](https://github.com/SerratedSharp/CSharpWasmRecipes/assets/97156524/5554e8a5-0a36-47d8-8a15-3a85b4bebff2)

### Instances and Instance Methods

.NET 7 provides the JSObject (in System.Runtime.InteropServices.JavaScript) type that represents a javascript object reference.  Think of this type as being similar to an `object`, in that it is not strongly typed and can hold a reference to any JS type.  Although the type exposes limited functionality, the ability to hold a reference to a JS Object from .NET and return/pass it across the interop layer opens up a multitude of capabilities.  Wrappers composed of JSObject references and proxy methods/properties can present a strongly typed interface for JS types.  For example, SerratedJQ's JQueryPlainObject is a strongly typed wrapper for JQuery objects and internally uses a JSObject for the reference to the JQuery object instance.

Because .NET 7 doesn't currently support importing JS instance methods directly, we declare a static method in JS and import it into C#, with the first parameter being the JS instance we want to operate on.

Note: VS2022 can often automatically add Uno.Foundation.Interop using the incorrect JSObject causing confusing compilation errors.

#### JSObject Wrappers

Let's look at developing an interface for interacting with JS types from C#.  We'll use vanilla HTML elements and HTML DOM methods as our example, but this same approach can be applied to custom JS types.  This demonstrates one opinionated approach to mapping JS instance methods, but demonstrates the fundamentals that would be used in some form by most approaches.

Declaring static javascript methods:
```js
// findElement takes a string and returns an object (an HTML element reference)
// getClass takes an object, calls an instance method on that object, and returns a string
globalThis.findElement = function(id) { return document.getElementById(id); }
globalThis.getClass = function(obj) { return obj.getAttribute('class'); }
```

The `getClass` method takes a JS object as a parameter, then calls an instance method on it.

Import static JS methods into C#:
```C#
using System.Runtime.InteropServices.JavaScript;
public partial class HtmlElementProxy
{
    [JSImport("globalThis.findElement")]
    public static partial JSObject FindElement(string id);

    [JSImport("globalThis.getClass")]
    public static partial string GetClass(JSObject obj);
```

Then call the methods from C# WASM through the JSObjectExample C# proxy, passing the JSObject reference into it so it can call an instance method on it:
```C#
// Call the static JS functions from C#
JSObject element = HtmlElementProxy.FindElement("uno-body");
// Pass the handle to another method
var elementClasses = HtmlElementProxy.GetClass(element);
Console.WriteLine("Class string: " + elementClasses);
```

Now any static method can implement instance semantics by taking a JSObject as its first parameter, and in turn calling the instance method on the JSObject.  

To create an interface that more closely resembles the native JS type with instance methods, we'll implement another layer to act as a wrapper.  We'll also split static methods and instance methods into separate classes.

```C#
public static class HtmlDomJS // static methods for interacting with the HTML DOM
{
    public static HtmlElementObject FindElement(string id){
        return new HtmlElementObject( HtmlElementProxy.FindElement(id));
	}
}

public class HtmlElementObject // a strongly typed instance wrapping JSObject reference to an HTML element
{
	public JSObject Handle { get; private set; }
	public HtmlElementObject(JSObject handle) { Handle = handle; }

	public string GetClass() => HtmlElementProxy.GetClass(Handle);
}

//Usage:
HtmlElementObject element = HtmlDomJS.FindElement("uno-body");
string elementClasses = element.GetClass();
```

Now we can work with HtmlElementObject using instance semantics we are more familiar with in .NET.  The JS object's lifetime will be dependent on the C# object.  If the C# object goes out of scope and is garbage collected, then the JS object will become available for garbage collection within JS (assuming no other JS references it).

Let's review the above starting with the deepest JS layer and working up to C#:
- Javascript Declaration: `globalThis.getClass`
  - Takes an instance of a JS object(HtmlElement) as a parameter, and calls an JS instance method on it.  If this were a JS library wrapper, this is where we would call a JS library specific method on the instance.  
  - Necessary because `[JSImport]` cannot directly import instance methods, so a static method declaration is needed.
  - Not necessary when mapping existing static JS methods, but could still be useful for coercing types or performing other mapping within JS.
  - JS instance method declarations can be eliminated and replaced with [Universal JS Instance Method](#Universal-JS-Instance-Method) or SerratedSharp.JSInteropHelpers
- Interop Proxy: Static C# class declaration `HtmlElementProxy`
  - Proxies/marshals the call from C# to JS across the interop boundary, and function signatures must match the JS method signatures with compatible types.  Other parameters can be added to the method signature, but by convention we use the first parameter as the instance to operate on.  *Consider splitting such a class into HtmlElementStaticProxy containing static methods such as FindElement and HtmlElementInstanceProxy containing instance methods such as GetClass.*
  - When mapping only static methods, other layers could be omitted, but this layer would still be necessary.
  - Instance method proxies are implemented generically in SerratedSharp.JSInteropHelpers
- .NET Wrappers: C# class declaration `HtmlElementObject` and static `HtmlDomJS`
  - The static `HtmlDomJS` class handles the JSObject reference returned internally, constructing a wrapping HtmlElementProxy.  Having this additional layer also provides an opportunity for us to define a C# function signature that more closely matches .NET semantics, whereas `HtmlElementProxy` is forced by `[JSImport]` to use method signatures matching the JS method signatures.  Additionally, due to the limited types supported by JS interop, we may need to coerce types to native .NET types or vice versa at this layer.  
  - The instance declaration  `HtmlElementObject` encapsulates a JSObject which is handled internally to avoid exposing the loosely typed JSObject to consumers.  We choose to provide a public `Handle` property for consumer's edge cases where they may need the native JS reference for their own JS interop methods if need be.
  - Necessary if presenting strongly typed instance semantics is desired.

Using this approach, the HtmlElementProxy would be an internal/private implementation with HtmlDomJS and HtmlElementObject exposing the functionality publicly.  The \*JS suffix is used on the static class to indicate it wraps JS declarations, and calls to it result in JS interop calls.  The \*Object suffix on the container for the JSObject indicates it is a wrapper for a JS type, holds a reference to a JS type, and calls result in interop calls.  The naming convention is arbitrary, but is akin to suffixing classes where they represent proxies to other systems and hold unmanaged resources or calls that pass beyond the .NET runtime.

The JS and proxy layers for instance methods can be eliminated and handled generically with SerratedSharp.JSInteropHelpers. See [Generic Instance Proxy](#Generic-Instance-Proxy)

#### Memory Management of JSObject References

JSObject implements IDisposable which serves to release memory in the JS layer.   While unreferenced instances will be disposed automatically during a garbage collection, it is non-deterministic since the garbage collector may not run in a timely manner as JS memory pressure is not communicated to the .NET runtime's garbage collector.  Wrappers for types that represent large or numerous JS allocations should implement IDisposable, proxy the calls to JSObject, and usage of the type should follow deterministic disposal patterns with `using` blocks.

#### Universal JS Instance Method

To reduce the amount of boilerplate JS code needed to map instance methods, we can use .apply to call JS methods of an arbitrary name and number of parameters [Mozilla Function.prototype.apply\(\)](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Function/apply): 

```js
InstanceProxy.FuncByNameToObject = function (jsObject, funcName, params) {
    const rtn = jsObject[funcName].apply(jsObject, params);    
    return rtn;
};
```

```C#
public static partial class JSInstanceProxy
{
    private const string baseJSNamespace = "globalThis.Serrated.InstanceProxy";

    [JSImport(baseJSNamespace + ".FuncByNameToObject")]
    [return: JSMarshalAs<JSType.Any>]
    public static partial
        object FuncByNameAsObject(JSObject jqObject, string funcName, [JSMarshalAs<JSType.Array<JSType.Any>>] object[] parameters);
}

//Usage:
FuncByNameAsObject(elementJSObject, "getAttribute", new object[] { "class" });
```

#### Proxyless Instance Wrapper

A generic approach to replace the JS and proxy layers is implemented in SerratedSharp.JSInteropHelpers, which leverages the `jsObject[funcName].apply()` technique in combination with mechanisms such as `[CallerMemberName]` to automatically call a JS function of the same name and parameters as the calling method's name.  For example, `Last() => this.CallJSOfSameNameAsWrapped();` will call a JS method named `last()` on the instance of JSObject this type wraps.  The IJSObjectWrapper interface is leveraged to access the JSObject handle that these instance methods operate on.
This approach is demonstrated in [JQueryPlainObject.cs](https://github.com/SerratedSharp/SerratedJQ/blob/main/SerratedJQLibrary/SerratedJQ/Plain/JQueryPlainObject.cs) which wraps a javascript jQuery object with a strongly typed C# wrapper.  Each of the below instance methods calls a JS method of the same name and parameters without the need to declare proxy JS or C# method/classes.  Various overloads support variations of returning a JSObject wrapper or primitive value.

```C#
using SerratedSharp.JSInteropHelpers;
public class JQueryPlainObject : IJSObjectWrapper<JQueryPlainObject>, IJQueryContentParameter
{
    internal JSObject jsObject;//  Handle to JS object, marked internal so other complementary static factory classes can wrap instances
    public JSObject JSObject { get { return jsObject; } }

    // Most constructers aren't called directly by consumers, but thru static methods such as .Select
    internal JQueryPlainObject() { }

    // Construct wrapper from JS object reference, not typically used and requires caller ensure the referenced instance is of the appropriate type
    public JQueryPlainObject(JSObject jsObject) { this.jsObject = jsObject; }

    // Factory constructor for interface used by JSInteropHelpers when methods return a new wrapped instance
    static JQueryPlainObject IJSObjectWrapper<JQueryPlainObject>.WrapInstance(JSObject jsObject)
    {
        return new JQueryPlainObject(jsObject);
    }

    // Map instance methods to JS methods of the same name and parameters using JSInteropHelpers:
    public JQueryPlainObject Last() => this.CallJSOfSameNameAsWrapped();
    public JQueryPlainObject Eq(int index) => this.CallJSOfSameNameAsWrapped(index);
    public JQueryPlainObject Slice(int start, int end) => this.CallJSOfSameNameAsWrapped(start, end);
    public bool Is(string selector) => this.CallJSOfSameName<bool>(selector);
    public string Attr(string attributeName) => this.CallJSOfSameName<string>(attributeName);
    public void Attr(string attributeName, string value) => this.CallJSOfSameName<object>(attributeName, value);
    public string Val() => this.CallJSOfSameName<string>();
    public T Val<T>() => this.CallJSOfSameName<T>();
    //...
}

public static class JQueryPlain
{
    // Example of a static method returning newly constructed/wrapped JS references
    public static JQueryPlainObject Select(string selector)
    {
        var managedObj = new JQueryPlainObject();// Leverages internal constructor
        // Then explicitly sets the JSObject reference retrieved through JS proxy
        // Note, using this approach static methods still have explicit proxies
        managedObj.jsObject = JQueryProxy.Select(selector);
        return managedObj;
    }
}
```

## Promises

Demonstrations include approaches to exposing a JS promise, async method, or old style callback as an async method in C# that can be awaited.  Demonstrates awaiting RequireJS dependency resolution where C# code needs to wait for a JS dependency to load.

### C# Awaiting a JS Promise

```JS
globalThis.functionReturningPromisedString = function (url) {   
    return fetch(url, {method: 'GET'})  // request URL
        .then(response => response.text()); 
    // Note that .text() returns a promise.
}   
```

```C#
internal static partial class RequestsProxy
{
    // Match the above javascript function signature.
    [JSImport("globalThis.functionReturningPromisedString")]
    [return: JSMarshalAs<JSType.Promise<JSType.String>>()] // JS function returns a promise that resolves to a string
    public static partial Task<string> // the return type Task<string> corresponds to the marshaled Promise<string>
        FunctionReturningPromisedString(string url);
}

// Usage:
string response = await RequestsProxy.FunctionReturningPromisedString("https://www.example.com");
```

### C# Awaiting an Event Exposed as a JS Promise

Sometimes it is necessary to guarantee that an operation has completed before continuing execution, but some older JS APIs only signal completion using JS events rather than returning promises.  We can wrap the event in a new JS promise, and then C# code can await the promise.  Note this is not appropriate for all events.  See **Events** section for methods of subscribing to JS events from C#.

This JS creates a `<script>` element to load a javascript file, and exposes the `onload` event as a promise that resolves when the script is loaded.  This is useful for awaiting the loading of a JS library.

The key points of this approach:
- Create a new Promise
- Assign the resolve parameter as the handler for the event we wish to await
- Return the new promise

Callers can await the promise and it will await until the event is raised.

```JS
globalThis.LoadScript = function (relativeUrl) {
    return new Promise(function (resolve, reject) {
        var script = document.createElement("script");
        script.onload = resolve;
        script.onerror = reject;
        script.src = relativeUrl;
        document.getElementsByTagName("head")[0].appendChild(script);
    });
};
```

```C#
internal static partial class HelpersProxy
{
    private const string baseJSNamespace = "globalThis";
    [JSImport(baseJSNamespace + ".LoadScript")]s
    public static partial Task
        LoadScript(string relativeUrl);
}

// Usage:
await HelpersProxy.LoadScript("https://code.jquery.com/jquery-3.7.1.min.js");
```

### Conditional Promises

Methods that can return promises should consistently return a promise for all code paths.  If you have logic where certain circumstances do not need to execute awaitable code and should return immediately, then just return a resolved promise. This is necessary so that all code paths return some form of a promise for consistent handling by the caller:

```JS
HelpersProxy.LoadjQuery = function (relativeUrl) {
    if (window.jQuery) {
        // jQuery already loaded and ready to go, nothing to await
        return Promise.resolve();// resolve immediately
    } else {
        return HelpersProxy.LoadScript(relativeUrl);
    }
};
```

## Events

### Listening to JS Events with C# Action/Method Handlers

Declare a static JS function to proxy the call to `addEvenetListener`:
```JS
globalThis.subscribeEvent = function(elementObj, eventName, listenerFunc) { 
    return elementObj.addEventListener( eventName, listenerFunc, false ); 
}
```

Import the static JS function into C#:
```C#
public partial class EventsProxy
{
    [JSImport("globalThis.subscribeEvent")]
    public static partial string SusbcribeEvent(JSObject elementObj, string eventName, 
        [JSMarshalAs<JSType.Function<JSType.Object>>] Action<JSObject> listener);
}
```

Pass an action method that will act as the event listener: 
```C#
JSObject element = SObjectExample.FindElement("#someBtnId"); // leverage our interop method implemented in other examples

EventsProxy.SusbcribeEvent(element, "click", 
    (JSObject eventObj) => {
        Console.WriteLine($"Event fired with event type via interop property '{eventObj.GetPropertyAsString("type")}'");
        JSObjectExample.Log(eventObj);
    });
```

Our C# event handler will be triggered each time the button element is clicked.

We pass our event handler to the JSImport'd method's third parameter:
```
[JSMarshalAs<JSType.Function<JSType.Object>>] Action<JSObject> listener
```

As demonstrated above, we can access properties of the event parameter `JSObject eventObj` using JSObject's.GetPropertyAs\* methods.  For example, we might use `eventObj.GetPropertyAsJSObject("target")` to retrieve `event.target` as a reference to the HTMLElement, then pass this to other interop methods that might change the state of the button or retrieve data from a parent form.

Approaches of interacting with the event object (some of which covered in examples elsewhere in this document):
- Calling the JSObject's GetPropertyAs\* on the `eventObj`
- Calling a JSImport'd method and passing the eventObj as a parameter, and allowing the JS shim to access or operate on the parameters.
- Wrapping our `listenerFunc` in the JS shim implementation to either fully or partially serialize the eventObj to a JSON string before passing it the C# event handler where it can be deserialized.  This may have undesirable side affects since serializing a property such as event.currentTarget will lose it's reference as an HTMLElement.
- Wrapping our `listenerFunc` in the JS shim implementation, extracting additional values from the eventObj or DOM, and passing them as additional parameters to our event listener.  Requires our event listener be declared with additional parameters.

SerratedJQ uses an advanced approach, where it partially serializes the event object, and uses a visitor pattern to insert replacement placeholders and a preserve references to HTMLElement/jQueryObject references in an array.  An intermediate listener deserializes the JSON, and restores the JSObject references.  This hybrid approachs allows most primitive values of the event to be accessed naterually without interop, while specific references such as target/currentTarget properties can be acted on as JSObject's.  This is required where we would want to interact with \*.currentTarget's HTMLElement through interop.

#### Decomposing Event Parameters in the JS Shim


JS events can be exposed as classic C# events to present them using C# semantics.  SerratedJQ demonstrates this approach with JQueryPlainObject.OnClick and other similar events.

#### Instance Methods as Event Listeners

An instance method can be used as an event listener.:

```C#
public class SomeClass
{
    // SubscribeEvent click listener instance function
    public void InstanceClickListener(JSObject eventObj)
    {
        Console.WriteLine($"[ClickListener handler] Event fired with event type via interop property '{eventObj.GetPropertyAsString("type")}'");
        JSObjectExample.Log(eventObj);
    }
}

// Usage:
var instance = new SomeClass();
EventsProxy.SusbcribeEvent(element, "click", instance.InstanceClickListener); // Using instance method as listener
```

- Allows the handler to access state of the C# instance, allowing backing state/model data to be managed in C#.  Useful for cases where you may have a list of UI items and the listener needs to act on the backing data specific to the item clicked.
- Ties the lifetime of the C# instance to the lifetime of the HTMLElement.  As long as the C# instance is subscribed to the event, and the JSObject reference publishing the event has not been garbage collected, then the C# instance will be preserved.
- Allows the instance to expose a traditional C# event and present strongly typed C# data for the event, hiding the interop details from the downstream event consumer.

For example, this can be valuable when implementing a web component wrapper in C# that encapsulates an HTMLElement:
- This is demonstrated in the SerratedJQSample project's [ProductSaleRow](https://github.com/SerratedSharp/SerratedJQ/blob/d2406a1b94334f6fc3ceba422e74f25d289004bb/SerratedJQSample/Sample.Wasm/ClientSideModels/ProductSaleRow.cs#L28) which wraps an HTML fragment as a component, proxies JS events, and includes backing model data in the event specific to that row/instance.  This allows downstream consumers of the component to subscribe to a strongly typed event which includes the model data, and hides the complexities of JS interop from the caller.

#### Alternative Syntaxes for Declaring the Event Listener

```C#
// Using local Action variable as listener
Action<JSObject> listener = (JSObject eventObj) =>
{
    Console.WriteLine($"[Local variable handler] Event fired with event type via interop property '{eventObj.GetPropertyAsString("type")}'");
    JSObjectExample.Log(eventObj);
};
EventsProxy.SusbcribeEvent(element, "click", listener); 
            
EventsProxy.SusbcribeEvent(element, "click", ClickListener); // Using static function as listener

///...
internal class Program
{
    // SubscribeEvent click listener static function
    private static void ClickListener(JSObject eventObj)
    {
        Console.WriteLine($"[ClickListener handler] Event fired with event type via interop property '{eventObj.GetPropertyAsString("type")}'");
        JSObjectExample.Log(eventObj);
    }
}
```

### Wrapping a JS Event as a C# Event


### Triggering JS Events from C#




TODO: Demonstrate limitation and workaround for passing arrays through events.

## JS Declarations

Many examples assign methods to `globalThis` for simplicity, but actual implementations should place JS declarations in a dedicated namespace or ES6 module to avoid naming conflicts with other libraries.  Modules are typically the more modern and recommended approach.

One approach to creating dedicated namespaces without modules is using the IIFE (Immediately Invoked Function Expression) convention:

```JS
var Your = globalThis.Example || {}; // declare parent namespace if not declared yet
(function (Example) {

    var SomeProxy = Example.SomeProxy || {};// create child namespace if not declared yet

    SomeProxy.LogMessage = function (arrayObject) {
        console.log('did something');
    }
    
    SomeProxy.AnotherMethod = function (arrayObject) {
        return 1+2;
    }

    Example.SomeProxy = SomeProxy; // add child to parent namespace

})(Example = globalThis.Example || (globalThis.Example = {}));
```

```C#
internal static partial class SomeProxy
{
    private const string baseJSNamespace = "globalThis.Example.SomeProxy";

    [JSImport(baseJSNamespace + ".LogMessage")]
    public static partial Task
        LogMessage(string relativeUrl);

    [JSImport(baseJSNamespace + ".AnotherMethod")]
    public static partial Task
        AnotherMethod(string relativeUrl);
}
```


# Archived
Approaches largely superseded by more recent capabilities/approaches.

## Mapping Static JS Methods or Executing Arbitrary JS Using WebAssemblyRuntime (Discouraged/Legacy)

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
public static class GlobalProxyJS
{
    public static void AlertProxy(string message)
    {
	    WebAssemblyRuntime.InvokeJS($"""globalThis.alertProxy("{message}");"""); 
    }
}
//... called from WASM Program.cs
GlobalProxyJS.AlertProxy("Hello World");
```

> [!IMPORTANT] 
> This approach should be avoided due to security risks of interpolating strings into javascript, where a string might be derived from user generated data upstream.  Mitigation is possible, but it is a potential security pitfall.

## Async/Promises with WebAssemblyRuntime.InvokeAsync (Discouraged/Legacy)

PromisesWUno demonstrates legacy approaches to calling JS methods that return promises, and awaiting them in C#.

## Security.cs
Demonstrates the risks of unencoded strings when building and executing javascript dynamically, and how to properly encode and fence string parameters passed from C# to JS to avoid XSS attacks that would attempt to abuse `InvokeJS`.  Note: This specific risk is not present when using .NET 7's JSImport/JSExport since javascript is not composed dynamically.

In general, the following guidelines should be followed:
 - User generated strings should only be embedded as parameters.
 - User generated strings passed as string parameters in JS must be escaped with EscapeJs.
 - User generated strings must be fenced in unescaped double quotes
 -- (the order you apply the escaping and then wrap in quotes is important)        

Note: As with any consideration for XSS, the phrase "user generated strings" covers a wide variety of scenarios where a string potentially originated from a user or other untrusted source upstream.  For example, in a given context, you may have queried a string from a trusted API or trusted database, but the string may be untrusted because it was at some point submitted and saved from user input.  Consider where user's provide a description of themselves, and that description is stored in the DB, and can be viewed by other users.  A variety of attacks could be embedded in this string, such as HTML injection or JS injection, but could also contain legitimately benign characters that aren't malicious in nature, yet could interfere in a context, and yet should also be rendered correctly.  No sanitization is universal in protecting from all of these attacks without potentially mangling the string.  You must apply the appropriate encoding based on the context where the content is being embedded to ensure it is both secure and rendered correctly.  You may need to apply multiple encodings in a specific order based on nested contexts.  See `Security.SafeInvokeJSWithHtml()`

 Note: Use of JSImport rather than InvokeJS can eliminate the need for JS encoding parameters.  It does not absolve you of addressing other encoding contexts.

#### Using WebAssemblyRuntime IJSObject and InvokeJSWithInterop (Discouraged/Legacy)

Whenever an instance implementing IJSObject is created and we call JSOjbectHandle.Create(), a javascript object is created and tracked.  If an instance of IJSObject is passed directly into InvokeJSWithInterop via string interpolation, the corresponding javascript object is used in its place.  We choose an arbitrarily named `.obj` property to hold and access the javascript object we are wrapping.  The major benefit of this approach is we don't need to scaffold both javascript and C# static methods to act as proxies for instance semantics.

```C#
using Uno.Foundation.Interop;
using Uno.Foundation;
public class ElementWrapper : IJSObject
{
    internal JSObjectHandle handle;    
    JSObjectHandle IJSObject.Handle { get => handle; }
    // internal constructor, objects are created/returned only from static methods
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

Using the wrapper, we can retrieve an instance and call instance methods directly:
```C#
ElementWrapper elementWrapper = ElementWrapper.GetElementById("uno-body");
string elementClasses2 = elementWrapper.GetClass();
```


