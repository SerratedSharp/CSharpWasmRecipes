Access the table of contents via the ![image](https://github.com/SerratedSharp/CSharpWasmRecipes/assets/97156524/49113928-1bd7-4e7c-8c28-b1aafa035744) icon in the upper right of this pane.

To improve readability, navigate [here](README.md) then collapse the files drawer:

![image](https://github.com/SerratedSharp/CSharpWasmRecipes/assets/97156524/b850c806-7631-40b2-ac8d-e7cb4a10b386)

# C# WASM Recipes
Code snippets and guidance for C# WebAssembly(WASM), primarily focused on platform agnostic approaches that do not necessarily require the Blazor nor Uno Platform UI frameworks.  Optionally Uno.Wasm.Bootstrap can be used independently of the Uno Platform UI framework to provide compilation to a WASM compatible format, or alternatively the .NET 8 wasm-experimental workload providing the WebAssembly Browser App project template can be used. Otherwise code and techniques focus on those that rely on .NET 7 JS interop capabilities, thus would work with or without either framework.

This allows a C# WASM module to provide client-side capabilities in the context of virtually any web application, regardless of server side hosting technologies or client-side frameworks, so long that it runs in a modern standards compliant browser.

(Portions of this article were adapted and submitted for inclusion in the ASP.NET Core reference documentation.  I am the original author of the following content, and is provided here to provide additional context, examples, and guidance.)

## Examples Project

The majority of examples from this article are implemented in the WasmBrowesr.Recipes.WasmClient project: [C# Code](https://github.com/SerratedSharp/CSharpWasmRecipes/tree/main/WasmBrowser.Recipes.WasmClient/Examples) and [JavaScript Code](https://github.com/SerratedSharp/CSharpWasmRecipes/tree/main/WasmBrowser.Recipes.WasmClient/wwwroot).

# C# WASM Overview

When a .NET project is compiled to WebAssembly(WASM), the resulting package can be run in a browser that supports the WebAssembly runtime standard.  Unlike legacy proprietary technologies such as Silverlight which failed to achieve ubiquitous browser support, WebAssembly has been adopted as a web standard and is supported by all major browsers: [Supported Browsers](https://developer.mozilla.org/en-US/docs/WebAssembly#browser_compatibility).

Compiling C#/.NET to the WebAssembly format enables several capabilities and benefits:

- Run C#/.NET in the browser client-side using a secure sandbox.
- Allows client-side or UI logic to be implemented in C# in lieu of Javascript.
- Ability to leverage the ecosystem of .NET frameworks and libraries to implement client-side logic.
- Implement reusable logic in C# which can be leveraged by existing Javascript implementations.
- Improve developer productivity by reducing the impact of context switching between C# and Javascript.
- Reduce duplication of logic such as API models and validation logic which would previously have been implemented both server-side and client-side in C# and Javascript respectively.
- Run intensive client-side code with close to native performance in a precompiled AoT (ahead-of-time) format.
- Reduce server load where processing such as HTML templating can be offloaded client-side.
- Expands the ecosystem of code/logic sharing across platforms and programming languages.

Note that a C# WASM package can be used client-side with any server side technology, and is not limited to ASP.NET.  The compiled WASM package is served and downloaded to the browser as static files, and then executed client-side.  For example, a github.io page which only supports static content can serve a C# WASM package.

In the case of WASI, where WebAssembly can be executed in other contexts besides the browser, much of the above applies and additionally:

- Eliminates the need to have the .NET runtime installed on the target system.
- Greater portability, allowing platforms and programming languages from diverse ecosystems to leverage a common execution model.
- Reduce dependence on fragmented runtime/execution tooling.
- Eliminate the need for tools/libraries/packages to be compiled locally to achieve portability.
- Improve interoperability across tools/libraries.
- Collectively reduce duplication of ubiquitous algorithms across platforms/languages.

Note: I use the phrase "C# WASM" colloquially to refer to C# code compiled within a WebAssembly module.  It would be more accurate to refer to it as ".NET WASM", as any compatible .NET language could be used.

## Interop Supporting Runtimes & Frameworks

It is helpful to be familiar with the ecosystem of different libraries and frameworks that support WebAssembly or provide JS interop capability within .NET.
  
### WebAssembly Module Runtimes

To compile a .NET project to a WebAssembly module, one of the following project types can be used.  In addition to compiling your .NET code to the WebAssembly format to be executed by the browser, they both include the JavaScript and .NET runtimes needed to load or "bootstrap" your WebAssembly module.  While the browser provides WASM execution support, some orchestration is needed from JS scripts to download related modules and other assets needed to initialize and load the WASM hosted .NET runtime and your WebAssembly module.  Both of the following project types produce a package of files upon project Publish which include all of the necessary web assets and JavaScript to handle the runtime initialization process.

- **wasm-experimental/WASM Browser**: The .NET 8 wasm-experimental workload provides the WebAssembly Browser App project template.  We'll refer to this as WASM Browser.  When compiled it produces a WASM package, is structured as a console app with a Main method that is called when loaded in browser, and the project by default will launch a self hosted web server for development/debugging locally.  The resulting package can be published to a folder, and the containing assets deployed to any web server that can serve static files.  Installation instructions: [Run .NET from JavaScript - Prerequisites](https://learn.microsoft.com/en-us/aspnet/core/client-side/dotnet-interop?view=aspnetcore-8.0#prerequisites).
- **Uno.Bootstrap.Wasm**: Tooling for compiling and packaging a .NET assembly as a WebAssembly/WASM package, along with all the JavaScript necessary for loading(i.e. **bootstrap**ping) the WASM into the browser.  This is only intended for use in the root project which will be the entry point for the WebAssembly.  Other class libraries/projects referenced do not need to reference this package.   (Note, no relation to the Bootstrap CSS/JS frontend framework.)  The name "Bootstrap" refers to similar terminology used for loading operating systems, as it "pulls itself up by its bootstraps".  This project type does not include the Uno UI Platform, but rather produces a simple module for executing .NET code.

Either of the above project types are compatible with JSImport/JSExport and provide similar capabilities to run .NET code and integrate with JS, HTML DOM, or other browser APIs.  Uno Bootstrap has been at a stable release for a few years, while WASM Browser as of .NET 8 is fairly new and considered experimental.  I personally prefer WASM Browser as there are fewer integration and configuration pitfalls compared to Uno.Bootstrap.Wasm.

Other .NET UI platforms such as Blazor or Uno UI Platform include WASM as a component of their platform and also support code using JSImport/JSExport(when executing in a Javscript host/browser context).

### .NET/JavaScript Interop Libraries

Class libraries which facilitate interop between .NET code and JavaScript.  Typically only one of these would be used in a given project, although it is possible to use multiple.

- **.NET 7, System.Runtime.InteropServices.JavaScript**: .NET 6 added many JS interop capabilities within `System.Runtime.InteropServices.JavaScript` that are compatible with multiple platforms, and were expanded in .NET 7.  These are compatible with Uno.Bootstrap.Wasm, .NET 8 WASM Browser, and Blazor.  .NET 6/7's interop is a significant improvement over prior approaches:
  - Prior approaches required JS objects or managed types to be explicitly marshaled which varied from easy to impossible in difficulty depending on the scenario.  Parameter fencing had to be carefully guarded for security, and managed types had to sometimes be manually pinned/unpinned.
  - .NET 7 has native support for JS and managed object handles on both sides of the interop layer and ensures references across the interop boundary are not garbage collected prematurely.  Exporting and importing methods is more straightforward and passing references originating from JS is simplified.  **Overall using .NET 7's \*.InteropServices.JavaScript is the preferred approach.**
- **Uno.Foundation.WebAssemblyRuntime/Uno.Foundation.Interop**: This provides the majority of **legacy** interop capabilities for Uno.Bootstrap.Wasm prior to first class support for interop in .NET. Uno Boostrap now supports System.Runtime.InteropServices.JavaScript, and this is strongly recommended over using the legacy interop APIs.  As such, most of my examples for these approaches are archived.  Even when using Uno.Wasm.Bootstrap for compilation support, Nuget references to Uno.Foundation.WebAssemblyRuntime/Interop can be omitted.
- **Microsoft.JSInterop.IJSRuntime/JSRuntime**: Typically used by code specific to the Blazor framework.  *To my knowledge* this cannot be used in non-Blazor contexts such as Uno.Bootstrap.Wasm or WASM Browser without hooks into the Host initialization that would typically instantiate and register the JSRuntime into the DI ServiceCollection.  Note, client-side Blazor WASM components can opt to leverage System.Runtime.InteropServices.JavaScript.

### Additional Packages

- **Uno.Wasm.Bootstrap.DevServer**: If using Uno.Bootstrap.Wasm, this provides a self-hosted HTTP server for serving static WASM files locally and supporting debugging browser link to enable breakpoints and stepping through C# code in the IDE while it is running as WASM inside the browser.  This package is useful during local development, but would likely be eliminated when hosted in test/production, where you would likely package the WASM package and related JavaScript files to be served statically from a traditional web server.  - **Uno.UI.WebAssembly**: At one time this package generated some JavaScript declarations that WebAssemblyRuntime was dependent on. For example, `WebAssemblyRuntime.InvokeAsync()` would fail at runtime if this package had not been included. At least since 8.* release, this package is no longer required for vanilla WASM projects.
- **WasmAppHost**: WASM Browser project templates use WasmAppHost automatically for self-hosting during development to serve the WebAssembly's as static web assets.  Nothing should be needed aside from setting the WASM Browser project as the startup project to enable this.  [WASM App Host](https://github.com/dotnet/runtime/blob/main/src/mono/wasm/host/README.md)
- **SerratedSharp.JSInteropHelpers**: An optional library of helper methods for implementing interop useful for wrapping JS libraries/types. Reduces the amount of boilerplate code needed to call JS. This library is less refined. I created it to support my own JS interop implementations, but it has been key in allowing me to implement large surface areas of JS library APIs quickly.  See [Proxyless Instance Wrapper](#proxyless-instance-wrappers) and additional examples in SerratedJQ implementation: [JQueryPlainObject.cs](https://github.com/SerratedSharp/SerratedJQ/blob/main/SerratedJQLibrary/SerratedJQ/Plain/JQueryPlainObject.cs)
- **SerratedSharp.SerratedJQ**: An optional .NET wrapper for jQuery to enable expressive access and event handling of the HTML DOM from WebAssembly.  Many of the examples below use native DOM APIs to demonstrate the fundamentals of JS interop.  However, if your goal is DOM access/manipulation/event-handling, then much of the JS shims and C# proxies can be omitted by using [SerratedJQ](https://github.com/SerratedSharp/SerratedJQ) instead.  Additionally, the monadic nature of JQuery lends itself well to minimizing the number of interop object references or interop calls needed by not requiring collections to be materialized across the interop boundary and allowing bulk operations across multiple elements with a single interop call.

The WebAssemblyRuntime package or .NET 7 InteropServices namespace can also be used from class library projects implementing interop which is intended for consumption in a Uno.Bootstrap.Wasm or Wasm Browser project.  Typically a Class Library project template would be used, and would **not** reference Uno.Wasm.Bootstrap.  This template would produce a standard .NET assembly, but you would typically set `[assembly: System.Runtime.Versioning.SupportedOSPlatform("browser")]` in its AssemblyInfo.cs to indicate it is only intended for consumption in applications running as WebAssembly in a browser context.  The class library is not compiled directly to WASM, because the consuming root project is responsible for compiling and packaging all referenced assemblies into the final WASM package.  Class libraries can include Javascript files to act as interop shims.  However, the method for including them varies between whether the class library is consumed in a Uno.Bootstrap.Wasm or WASM Browser project.  For Uno.Bootstrap.Wasm, the JS files must be declared as AMD modules, are placed in a WasmScripts project folder, flagged as Build Action: Embedded Resource, and will be loaded automatically by Uno when.  For WASM Browser, the class library project is declared with `<Project Sdk="Microsoft.NET.Sdk.Razor">`, are placed in a wwwroot folder, flagged as Build Action: Content, and can be loaded at runtime using `JSHost.ImportAsync()`.  Additionally, the JSImport declarations must omit the module name for Uno, and include them for WASM Browser.  A class library can provide shims/proxies with each approach, and then conditionally call each based on whether the calling platform is Uno.Bootstrap.Wasm or not.

## Architecture and Debugging

The [Architecture](Architecture.md) (TODO: Needs updates for WASM Browser template) overview covers the structure of a new or existing website integrating a WebAssembly package, possible structures of Projects/Solution, and an overview of enabling debugging.  

### Debugging

The example WasmBrowser.Recipes.WasmClient project supports debugging breakpoints, allowing you to step through C# code running in the context of the WASM module in the browser.  Since the .NET code is actually being executed within the browser, debugging synchronization is supported through WebSocket communication between the browser and Visual Studio per the `inspectUri` setting in launchSettings.js.  In some cases you may observe execution pausing within the browser's debug console first.  In such a case, typically you would not choose the Continue or Resume option from within the browser, but instead wait a few seconds for synchonization, then the breakpoint in Visual Studio should then become highlighted.

For an example of using seperate projects for the client-side WASM module with Uno.Bootstrap.Wasm and a server-side ASP.NET site, with debugging wired up to support breakpoints in the WASM module, see [SerratedJQSample](https://github.com/SerratedSharp/SerratedJQ/tree/main/SerratedJQSample).  This includes project configuration for debugging in VS2022, and debugging setup is included in the SerratedJQ [Quick Start Guide](https://github.com/SerratedSharp/SerratedJQ#quick-start-guide).  SerratedJQ can be omitted, as the debugging setup is the same regardless.  Following the Quick Start guide while omitting SerratedJQ will result in a basic project setup for an ASP.NET MVC web application which includes a WebAssembly package for implementing client-side logic.

See the [Troubleshooting](#Troubleshooting) section for additional guidance.

## Loading JavaScript Declarations

JavaScript declarations such as JS shims which are intended to be access from .NET WASM interop would typically be loaded in the context of the same webpage or JavaScript host which loaded the .NET WebAssembly.   This could be accomplished by:
- A `<script>...</script>` tag declaring traditional JavaScript inline
- A `<script src='./some.js'>` tag loading a traditional external *.js file
- Loading a JavaScript ES6 module using `<script type='module' src="./moduleName.js"></script>` tag on the page
- Loading a JavaScript ES6 module *.js file by using `JSHost.ImportAsync(...)` from the .NET WebAssembly

Essentially, any JavaScript declarations available on the page prior to the .NET WASM module being loaded could be referenced by `JSImport`.  Additionally, the WASM module can load additional scripts on-demand by calling `JSHost.ImportAsync()`.

Examples in this article use `JSHost.ImportAsync(...)`.  When calling `JShost.ImportAsync(...)`, the client-side .NET WebAssembly will request the file using the `moduleUrl` parameter, and thus expects the file to be accessible as a static web asset much the same way as a `<script>` tag would retrieve a file with a `src` URL.  For example, if using the following C# code within a WebAssembly Browser App project, then the *.js file would be placed inside the project under `/wwwroot/scripts/ExampleShim.js`:

```C#
await JSHost.ImportAsync("ExampleShim", "/scripts/ExampleShim.js");
```

Note that depending on the platform loading the WebAssembly, a dot prefixed URL such as `./scripts/` might refer to an incorrect subdirectory such as `/_framework/scripts/` because the WebAssembly package is initialized by framework scripts under `/_framework/`.  In that case prefixing the URL with `../scripts/` would refer to the correct path, or prefixing `/scripts/` would work only if the site is hosted at the root of the domain.  A generic approach would typically involve configuring the correct base path for the given environment with an HTML `<base>` tag and using the `/scripts/` prefix to refer to the path relative to the base path.  **Tilde notation `~/` prefixes are not supported by `ImportAsync()`.**

> [!IMPORTANT] 
> If JavaScript is loaded from an ES6 module, then JSImport attributes must include the module name as the second parameter.  For example, `[JSImport("globalThis.callAlert", "ExampleShim")]` would indicate the imported method was declared in an ES6 module named "ExampleShim".

> [!NOTE] 
> Uno.Wasm.Bootstrap supports an alternative method of including JavaScript files, which should be declared as AMD modules.  These files will be included via RequireJS at runtime automatically.  See [Uno JavaScript Components - Embedding Assets](https://platform.uno/docs/articles/interop/wasm-javascript-1.html#embedding-assets).  This approach can also be used by a Class Library project which has JS files intended for consumption in a Uno.Bootstrap.Wasm project.  When using this technique, the module name parameter should **not** be included in the `[JSImport]` attribute.

> [!NOTE] 
> A Class Library for consumption in a WASM Browser project may use project type `<Project Sdk="Microsoft.NET.Sdk.Razor">` and place JS files in a wwwwroot folder of the Class Library.  This feature is known as Static Web Assets.  When a host WASM Browser project references the Class Library, then these files would be served under the path "_content/*ClassLibraryName*/".  The JS files can be loaded using `JSHost.ImportAsync("ExampleShim", "/_content/*ClassLibraryName*/ExampleShim.js");`, which requires the JS files contain ES6 compatible modules.

A class library can employ both techniques, embedding an AMD version of the JS file in /WasmScripts, and an ES6 version of the JS file available in /wwwroot.  This allows the Class Library to be consumed by either Uno.Bootstrap.Wasm or WASM Browser projects, and the appropriate version of the file will be loaded either automatically by Uno's use of requireJS, or explicitely by the caller using `JSHost.ImportAsync()`.  The Class Library should provide an initialization method that consumers will call in a WASM Browser usage scenario, which in turn calls ImportAsync for each of its dependencies with the appropriate subpath path, and prepending an optional caller provided base path parameter.

# Interoperate with JavaScript from .NET WebAssembly

This article explains how to use the API in the System.Runtime.InteropServices.JavaScript namespace to interact with JavaScript (JS) from client-side WebAssembly components that adopt .NET 7 or later.  This API is colloquially referred to as JSImport/JSExport interop, so named for the two most common attributes used to define desired interop.

This approach is applicable when running a .NET WebAssembly module in a JavaScript host such as a browser.  These scenarios include either Blazor WebAssembly client-side components as detailed in [JavaScript interop with ASP.NET Core Blazor](../blazor/js-interop/import-export-interop), non-Blazor .NET WebAssembly apps detailed in [Run .NET from JavaScript](dotnet-interop.md), and other .NET WebAssembly platforms which support JSImport/JSExport.  See the respective articles for examples specialized for these platforms.

## Prerequisites 

[.NET 7.0 SDK](https://dotnet.microsoft.com/download/dotnet/7.0): Install the latest version of the [.NET SDK](https://dotnet.microsoft.com/download/dotnet/).

A project using **one** of the following project types:

- A WebAssembly Browser App(WASM Browser) project setup according to [Run .NET from JavaScript](https://learn.microsoft.com/en-us/aspnet/core/client-side/dotnet-interop?view=aspnetcore-8.0) by completing [Prerequisites](https://learn.microsoft.com/en-us/aspnet/core/client-side/dotnet-interop?view=aspnetcore-8.0#prerequisites) and [Project Configuration](https://learn.microsoft.com/en-us/aspnet/core/client-side/dotnet-interop?view=aspnetcore-8.0#project-configuration).
    - Once you have installed the wasm-tools and wasm-experimental workloads, a new project template will be available in Visual Studio's new project dialog called "WebAssembly Browser App".
- A Console project referencing Uno.Wasm.Bootstrap setup according to [Uno.Bootstrap.Wasm - Using the Bootstrapper](https://platform.uno/docs/articles/external/uno.wasm.bootstrap/doc/using-the-bootstrapper.html).
- A Blazor client-side project setup according to [JavaScript JSImport/JSExport interop with ASP.NET Core Blazor](https://learn.microsoft.com/en-us/aspnet/core/blazor/javascript-interoperability/import-export-interop?view=aspnetcore-8.0).

This isn't an exhaustive list, as other commercial and/or open-source platforms exist which enable compiling .NET code to WebAssembly and support `System.Runtime.InteropServices.JavaScript`/`[JSImport]`.

# JS Interop using JSImport/JSExport
The JSImport attribute is applied to a .NET method to indicate that a corresponding JavaScript method should be called when the .NET method is called.  This all+ows .NET developers to define "imports" that enable .NET code to call into JavaScript.  Additionally, .NET Actions can be passed as parameters and JavaScript can invoke these .NET actions to support callback or event subscription patterns.

The JSExport attribute is applied to a .NET method to expose it to JavaScript code.  This allows JavaScript code to initiate calls to the .NET method.

## Importing & Calling JS Methods

This example imports an existing static JS method into C#.  JSImport is limited to importing static methods or instance methods of objects accessible globally.  For example, `.log()` strictly speaking is an instance method of the `console` object, but can be accessed using static semantics since the instance is a globally accessible singleton.

```C#
public partial class GlobalProxy
{
    // mapping existing console.log to a C# method
    [JSImport("globalThis.console.log")]
    public static partial void ConsoleLog(string text);
}

//... called from Program.cs Main() of a WASM project:
GlobalProxy.ConsoleLog("Hello World");// Output would appear in the browser console
```

The following demonstrates importing a static method declared in JavaScript.

Declaring a custom JS static method:
```JS
globalThis.callAlert = function (text) {
	globalThis.window.alert(text);
}
```

Mapping to a C# method proxy:
```C# 
using System.Runtime.InteropServices.JavaScript;

public partial class GlobalProxy
{
	[JSImport("globalThis.callAlert")]
	public static partial void CallAlert(string text);
}

//... called from WASM Program.cs Main():
GlobalProxy.CallAlert("Hello World");
```

Note that the C# class declaring the JSImport method does not have an implementation.  At compile time a source generated partial class will contain the .NET code which implements the marshalling of the call and types to invoke the corresponding JavaScript.  In Visual Studio, using the Go To Definition or Go To Implementation options will respectively navigate to either the source generated partial class or the developer defined partial class.

In this example, the intermediate `globalThis.callAlert` JavaScript declaration was used to wrap existing JavaScript. This article informally refers to the intermediate JavaScript declaration as a JS shim.  In this case it can "shim" or fill the gap between the .NET implementation and existing JS capabilities/libraries.  In many cases, such as this trivial example, the JS shim is not necessary and methods could be imported directly as is done in the ConsoleLog example.  As demonstrated later, sometimes a JS shim can serve to encapsulate additional logic, manually map types, reduce the number of objects or calls crossing the interop boundary, and/or manually map static calls to instance methods.

## Type Mappings

Parameters and returns types in the .NET method signature will automatically be converted to/from appropriate JS types at runtime if a unique mapping is supported.  This may result in values being converted by value, or references being wrapped in a proxy type. This process is known as type marshalling. Use `JSMarshalAsAttribute` to control how the imported method parameters and return types are marshalled. 

Some types do not have a default type mapping.  For example, a `long` can be marshalled as `JSType.Number` or `JSType.BigInt` and thus the `[JSMarsalAsAttibute]` is required or a compile time error will be generated.  

Of note is support for the following type mapping scenarios:
- An `Action` or `Func` can be passed as parameters and are marshalled as callable JS functions. This allows .NET code to provide listeners to be invoked in response to JS callbacks or events.
- JS references and .NET managed object references can be passed in either direction, are marshaled as proxy objects, and are kept alive across the boundary until the proxy is garbage collected.
- Asynchronous JS methods or [JS promises](https://developer.mozilla.org/docs/Web/JavaScript/Reference/Global_Objects/Promise) are marshalled with a `Task` result and vice versa. 

Most of the marshalled types work in both directions, as parameters and as return values, on both imported and exported methods.  

The following table details supported type mappings:

[.NET 8 JS Interop Type Mapping](https://github.com/dotnet/AspNetCore.Docs/blob/main/aspnetcore/blazor/includes/js-interop/8.0/import-export-interop-mappings.md)

Note, some combinations of type mappings that require nested generic types in `JSMarshalAs` are not currently supported.  For example, attempting to materialize an array from a JS promise such as `[return: JSMarshalAs<JSType.Promise<JSType.Array<JSType.Number>>>()]` will generate a compile time error.  An appropriate workaround will vary depending on the scenario, but this specific scenario explored in the [Type Mapping Limitations](#type-mapping-limitations)] section.

## JS Primitives

Demonstrates JSImport leveraging type mappings of several primitive JS types, and use of JSMarshalAs where explicit mappings are required at compile time.

```JS
// PrimitivesShim.js
let PrimitivesShim = {};
(function (PrimitivesShim) {

    globalThis.counter = 0;

    // Takes no parameters and returns nothing
    PrimitivesShim.IncrementCounter = function () {
        globalThis.counter += 1;
    };

    // Returns an int
    PrimitivesShim.GetCounter = () => globalThis.counter;
    // Identical with more verbose syntax:
    //Primitives.GetCounter = function () { return counter; };

    // Takes a parameter and returns nothing.  JS doesn't restrict the parameter type, but we can restrict it in the .NET proxy if desired.
    PrimitivesShim.LogValue = (value) => { console.log(value); };

    // Called for various .NET types to demonstrate mapping to JS primitive types
    PrimitivesShim.LogValueAndType = (value) => { console.log(typeof value, value); };
    
})(PrimitivesShim);

export { PrimitivesShim };
```

```C#
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;

public partial class PrimitivesProxy
{    
    // Importing an existing JS method.
    [JSImport("globalThis.console.log")]
    public static partial void ConsoleLog([JSMarshalAs<JSType.Any>] object value);

    // Importing static methods from a JS module.
    [JSImport("PrimitivesShim.IncrementCounter", "PrimitivesShim")]
    public static partial void IncrementCounter();

    [JSImport("PrimitivesShim.GetCounter", "PrimitivesShim")]
    public static partial int GetCounter();

    // The JS shim function name doesn't necessarily have to match the C# method name
    [JSImport("PrimitivesShim.LogValue", "PrimitivesShim")]
    public static partial void LogInt(int value);

    // A second mapping to the same JS function with compatible type
    [JSImport("PrimitivesShim.LogValue", "PrimitivesShim")]
    public static partial void LogString(string value);

    // Accept any type as parameter. .NET types will be mapped to JS types where possible,
    // or otherwise be marshalled as an untyped object reference to the .NET object proxy.
    // The JS implementation logs to browser console the JS type and value to demonstrate results of marshalling.
    [JSImport("PrimitivesShim.LogValueAndType", "PrimitivesShim")]
    public static partial void LogValueAndType([JSMarshalAs<JSType.Any>] object value);

    // Some types have multiple mappings, and need explicit marshalling to the desired JS type.
    // A long/Int64 can be mapped as either a Number or BigInt.
    // Passing a long value to the above method will generate an error "ToJS for System.Int64 is not implemented." at runtime.
    // If the parameter declaration `Method(JSMarshalAs<JSType.Any>] long value)` is used, then a compile time error is generated: "Type long is not supported by source-generated JavaScript interop...."
    // Instead, map the long parameter explicitly to either a JSType.Number or JSType.BigInt.
    // Note there could potentially be runtime overflow errors in JS if the C# value is too large.
    [JSImport("PrimitivesShim.LogValueAndType", "PrimitivesShim")]
    public static partial void LogValueAndTypeForNumber([JSMarshalAs<JSType.Number>] long value);

    [JSImport("PrimitivesShim.LogValueAndType", "PrimitivesShim")]
    public static partial void LogValueAndTypeForBigInt([JSMarshalAs<JSType.BigInt>] long value);
}

public static class PrimitivesUsage
{
    public static async Task Run()
    {
        // Ensure JS ES6 module loaded
        await JSHost.ImportAsync("PrimitivesShim", "/PrimitivesShim.js");

        // Call a proxy to a static JS method, console.log("")
        PrimitivesProxy.ConsoleLog("Printed from JSImport of console.log()");

        // Basic examples of JS interop with an integer:       
        PrimitivesProxy.IncrementCounter();
        int counterValue = PrimitivesProxy.GetCounter();
        PrimitivesProxy.LogInt(counterValue);
        PrimitivesProxy.LogString("I'm a string from .NET in your browser!");

        // Mapping some other .NET types to JS primitives:        
        PrimitivesProxy.LogValueAndType(true);
        PrimitivesProxy.LogValueAndType(0x3A);// byte literal
        PrimitivesProxy.LogValueAndType('C');
        PrimitivesProxy.LogValueAndType((Int16)12);
        // Note: JavaScript Number has a lower max value and can generate overflow errors
        PrimitivesProxy.LogValueAndTypeForNumber(9007199254740990L);// Int64/Long 
        PrimitivesProxy.LogValueAndTypeForBigInt(1234567890123456789L);// Int64/Long, JS BigInt supports larger numbers
        PrimitivesProxy.LogValueAndType(3.14f);// single floating point literal
        PrimitivesProxy.LogValueAndType(3.14d);// double floating point literal
        PrimitivesProxy.LogValueAndType("A string");
    }
}
// The example displays the following output in the browser's debug console:
//       Printed from JSImport of console.log()
//       1
//       I'm a string from .NET in your browser!
//       boolean true
//       number 58
//       number 67
//       number 12
//       number 9007199254740990
//       bigint 1234567890123456789n
//       number 3.140000104904175
//       number 3.14
//       string A string
```

## JS Date

Demonstrates JSImport of methods which have a JS Date object as its return or parameter.  Dates are marshalled across interop by-value, meaning they are copied in much the same way as JS primitives.

Be aware that a [JS Date](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Date) is time-zone agnostic.   A .NET DateTime will be adjusted relative to its DateTimeKind when marshalled, but time zone information will not be preserved.  Consider initializing DateTime's with a DateTimeKind.Utc or DateTimeKind.Local consistent with the value it represents, which will avoid incoprrect adjustments that may occur if the default DateTimeKind.Unspecified is used.

```JS
// DateShim.js
let DateShim = {};
(function (DateShim) {
    
    DateShim.IncrementDay = function (date) {
        date.setDate(date.getDate() + 1);
        return date;
    };
    
    DateShim.LogValueAndType = (value) => {
        if (value instanceof Date) 
            console.log("Date:", value)
        else
            console.log("Not a Date:", value)
    };

    DateShim.FromDate = (date) => date;
        
})(DateShim);

export { DateShim };
```

```C#
using System;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

public partial class DateProxy
{   
    [JSImport("DateShim.IncrementDay", "DateShim")]
    [return: JSMarshalAs<JSType.Date>] // Explicit JSMarshalAs for a return type.
    public static partial DateTime IncrementDay([JSMarshalAs<JSType.Date>] DateTime date);
    
    [JSImport("DateShim.LogValueAndType", "DateShim")]
    public static partial void LogValueAndType([JSMarshalAs<JSType.Date>] DateTime value);

    [JSImport("DateShim.FromDate", "DateShim")]
    [return: JSMarshalAs<JSType.Date>]
    public static partial DateTime FromDate([JSMarshalAs<JSType.Date>] DateTime value);

}

public static class DateUsage
{
    public static async Task Run()
    {
        // Ensure JS ES6 module loaded
        await JSHost.ImportAsync("DateShim", "/DateShim.js");

        // Basic examples of interop with a C# DateTime and JS Date.
        // Demonstrates affect of DateTimeKind on conversion.

        Console.WriteLine($"{NL}Increment Day");
        DateTime date = new DateTime(1968, 12, 21, 8, 51, 0, DateTimeKind.Utc);
        Console.WriteLine($".NET Date:  {date}, Kind: {date.Kind}, ToUTC: {date.ToUniversalTime()}");        
        DateProxy.LogValueAndType(date);
        DateTime fromJS = DateProxy.IncrementDay(date);        
        DateProxy.LogValueAndType(fromJS);

        Console.WriteLine($"{NL}Timezone Example (Kind.Local)");
        date = new DateTime(1968, 12, 21, 8, 51, 0, DateTimeKind.Local);
        Console.WriteLine($".NET Date: {date}, Kind: {date.Kind}, ToUTC: {date.ToUniversalTime()}");
        DateProxy.LogValueAndType(date);        
        fromJS = DateProxy.FromDate(date);
        Console.WriteLine($".NET Date: {fromJS}, Kind: {fromJS.Kind}, ToUTC: {fromJS.ToUniversalTime()}");
        DateProxy.LogValueAndType(fromJS);
        Console.WriteLine($"Are times the same? {TimesAreClose(date, fromJS)}");

        Console.WriteLine($"{NL}Timezone Example (Kind.Utc)");
        date = new DateTime(1968, 12, 21, 8, 51, 0, DateTimeKind.Utc);
        Console.WriteLine($".NET Date:  {date}, Kind: {date.Kind}, ToUTC: {date.ToUniversalTime()}");
        DateProxy.LogValueAndType(date);
        fromJS = DateProxy.FromDate(date);
        Console.WriteLine($".NET Date:  {fromJS}, Kind: {fromJS.Kind}, ToUTC: {fromJS.ToUniversalTime()}");
        DateProxy.LogValueAndType(fromJS);
        Console.WriteLine($"Are times the same? {TimesAreClose(date, fromJS)}");

        Console.WriteLine($"{NL}Timezone Example (Kind.Unspecified) assigned local time.");
        date = new DateTime(1968, 12, 21, 8, 51, 0);
        Console.WriteLine($".NET Date:  {date}, Kind: {date.Kind}, ToUTC: {date.ToUniversalTime()}");
        DateProxy.LogValueAndType(date);
        fromJS = DateProxy.FromDate(date);
        Console.WriteLine($".NET Date:  {fromJS}, Kind: {fromJS.Kind}, ToUTC: {fromJS.ToUniversalTime()}");
        DateProxy.LogValueAndType(fromJS);
        Console.WriteLine($"Are times the same? {TimesAreClose(date, fromJS)}");

        // Kind.Unspecified times are assumed to be local when converted to UTC, and assumed to be UTC when converted to local.
        // Assigning a time that represents a UTC without setting Kind.Utc, then passing it to JS will result in an incorrect time.
        Console.WriteLine($"{NL}Timezone Example (Kind.Unspecified) assigned UTC time.");
        date = new DateTime(1968, 12, 21, 12, 51, 0);        
        Console.WriteLine($".NET Date:  {date}, Kind: {date.Kind}, ToUTC: {date.ToUniversalTime()}");
        DateProxy.LogValueAndType(date);
        fromJS = DateProxy.FromDate(date);
        Console.WriteLine($".NET Date:  {fromJS}, Kind: {fromJS.Kind}, ToUTC: {fromJS.ToUniversalTime()}");
        DateProxy.LogValueAndType(fromJS);
        Console.WriteLine($"Are times the same? {TimesAreClose(date, fromJS)}");
    }

    public static bool TimesAreClose(DateTime date1, DateTime date2)
    {
        if ((date2.ToUniversalTime() - date1.ToUniversalTime()).TotalSeconds < 10.0)
            return true;
        else 
            return false;
    }

    static string NL = Environment.NewLine;
}
// The example displays the following output in the browser's debug console:
// Increment Day
// .NET Date:  12/21/1968 8:51:00 AM, Kind: Utc, ToUTC: 12/21/1968 8:51:00 AM
// Date: Sat Dec 21 1968 03:51:00 GMT-0500 (Eastern Standard Time)
// Date: Sun Dec 22 1968 03:51:00 GMT-0500 (Eastern Standard Time)
// 
// Timezone Example(Kind.Local)
// .NET Date: 12/21/1968 8:51:00 AM, Kind: Local, ToUTC: 12/21/1968 1:51:00 PM
// Date: Sat Dec 21 1968 08:51:00 GMT-0500 (Eastern Standard Time)
// .NET Date: 12/21/1968 1:51:00 PM, Kind: Utc, ToUTC: 12/21/1968 1:51:00 PM
// Date: Sat Dec 21 1968 08:51:00 GMT-0500 (Eastern Standard Time)
// Are times the same? True
// 
// Timezone Example(Kind.Utc)
// .NET Date:  12/21/1968 8:51:00 AM, Kind: Utc, ToUTC: 12/21/1968 8:51:00 AM
// Date: Sat Dec 21 1968 03:51:00 GMT-0500 (Eastern Standard Time)
// .NET Date:  12/21/1968 8:51:00 AM, Kind: Utc, ToUTC: 12/21/1968 8:51:00 AM
// Date: Sat Dec 21 1968 03:51:00 GMT-0500 (Eastern Standard Time)
// Are times the same? True
// 
// Timezone Example(Kind.Unspecified) assigned local time.
// .NET Date:  12/21/1968 8:51:00 AM, Kind: Unspecified, ToUTC: 12/21/1968 1:51:00 PM
// Date: Sat Dec 21 1968 08:51:00 GMT-0500 (Eastern Standard Time)
// .NET Date:  12/21/1968 1:51:00 PM, Kind: Utc, ToUTC: 12/21/1968 1:51:00 PM
// Date: Sat Dec 21 1968 08:51:00 GMT-0500 (Eastern Standard Time)
// Are times the same? True
// 
// Timezone Example(Kind.Unspecified) assigned UTC time.
// .NET Date:  12/21/1968 12:51:00 PM, Kind: Unspecified, ToUTC: 12/21/1968 5:51:00 PM
// Date: Sat Dec 21 1968 12:51:00 GMT-0500 (Eastern Standard Time)
// .NET Date:  12/21/1968 5:51:00 PM, Kind: Utc, ToUTC: 12/21/1968 5:51:00 PM
// Date: Sat Dec 21 1968 12:51:00 GMT-0500 (Eastern Standard Time)
// Are times the same? True
// 
```


## JS Object References

Whenever a JS method returns an object reference, it is represented in .NET with the JSObject type.  The original JS object continues its lifetime within the JS boundary, while .NET code can access and modify it by reference through the JSObject.  While the type itself exposes a limited API, the ability to hold a JS object reference as well as return or pass it across the interop boundary enables a great deal of capabilities.

The JSObject provides methods to access properties, but does not provide direct access to instance methods.  As the `Summarize()` method demonstrates below, instance methods can be accessed indirectly by implementing a static method that takes the instance to be acted on as a parameter.

```JS
// JSObjectShim.js
let JSObjectShim = {};
(function (JSObjectShim) {

    JSObjectShim.CreateObject = function () {
        return {
            name: "Example JS Object",
            answer: 41,
            question: null,
            summarize: function () {
                return `The question is "${this.question}" and the answer is ${this.answer}.`;
            }
        };
    };
    
    JSObjectShim.IncrementAnswer = function (object) {
        object.answer += 1;
        // We don't return the modified object, since the reference is modified.
    };

    // Proxy an instance method call.
    JSObjectShim.Summarize = function (object) {
        return object.summarize();        
    };

})(JSObjectShim);

export { JSObjectShim };
```

```C#
using System;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;

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
//     {name: 'Example JS Object', answer: 41, question: null, Symbol(wasm cs_owned_js_handle): 5, summarize: }
//     {name: 'Example JS Object', answer: 42, question: null, Symbol(wasm cs_owned_js_handle): 5, summarize: }
//     {name: 'Example JS Object', answer: 42, question: 'What is the answer?', Symbol(wasm cs_owned_js_handle): 5, summarize: }
//     Summary: The question is "What is the answer?" and the answer is 42.
```

## Asynchronous Interop

Many JavaScript APIs are asynchronous and signal completion through either a callback, promise, or async method.  Ignoring asynchronous capabilities is often not an option, as subsequent code may depend upon the completion of the asynchronous operation, and thus must be awaited.

JS methods using the `async` keyword or returning a promise can be awaited in C# by a method returning a Task.  Note as demonstrated below, the `async` keyword is not used on the C# method with the JSImport attribute, because it does not use the `await` keyword within it.  However, consuming code calling the method would typically use the `await` keyword and be marked as `async` as demonstrated in the `PromisesUsage` example.

JavaScript with a callback, such as a `setTimeout()`, can be wrapped in a Promise before returning from JavaScript.  Wrapping a callback in a promise as demonstrated in `Wait2Seconds()` is only appropriate when the callback is called exactly once.  Otherwise, a C# Action can be passed to listen for a callback that may be called zero or many times, which is demonstrated in [Subscribing to JS Events](#Subscribing-to-JS-Events).

```JS
// PromisesShim.js
let PromisesShim = {};
(function (PromisesShim) {

    PromisesShim.Wait2Seconds = function () {
        // This also demonstrates wrapping a callback-based API in a promise to make it awaitable.        
        return new Promise((resolve, reject) => {
            setTimeout(() => {                
                resolve();// resolve promise after 2 seconds
            }, 2000);
        });
    };
    
    // Returning a value via resolve() in a promise
    PromisesShim.WaitGetString = function () {
        return new Promise((resolve, reject) => {
            setTimeout(() => {
                resolve("String From Resolve");// return a string via promise
            }, 500);
        });
    };

    PromisesShim.WaitGetDate = function () {        
        return new Promise((resolve, reject) => {
            setTimeout(() => {
                resolve(new Date('1988-11-24'))// return a date via promise
            }, 500);
        });
    };

    // awaitable fetch()
    PromisesShim.FetchCurrentUrl = function () {
        // This method returns the promise returned by .then(*.text())
        // and .NET in turn awaits the returned promise.
        return fetch(globalThis.window.location, { method: 'GET' })
            .then(response => response.text());
    };

    // .NET can await JS functions using the async/await JS syntax:
    PromisesShim.AsyncFunction = async function () {
        await PromisesShim.Wait2Seconds();
    };

    // A Promise.reject() can be used to signal failure, 
    // and will be bubbled to .NET code as a JSException.
    PromisesShim.ConditionalSuccess = function (shouldSucceed) {        
       
        return new Promise((resolve, reject) => {
            setTimeout(() => {
                if (shouldSucceed)                
                    resolve();// success
                else                     
                    reject("Reject: ShouldSucceed == false");// failure
            }, 500);

        });

    };
    
})(PromisesShim);

export { PromisesShim };
```


```C#
using System;
using System.Diagnostics;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;

public partial class PromisesProxy
{
    // Do not use the async keyword in the C# method signature. Returning Task or Task<T> is sufficient.

    // When calling asynchronous JS methods, we often want to wait until the JS method completes execution.
    // For example, if loading a resource or making a request, we likely want the following code to be able to assume the action is completed.

    // If the JS method or shim returns a promise, then C# can treat it as an awaitable Task.

    // For a promise with void return type, declare a Task return type:
    [JSImport("PromisesShim.Wait2Seconds", "PromisesShim")]
    public static partial Task Wait2Seconds();

    [JSImport("PromisesShim.WaitGetString", "PromisesShim")]
    public static partial Task<string> WaitGetString();

    // Some return types require a [return: JSMarshalAs...] declaring the
    // Promise's return type corresponding to Task<T>.
    [JSImport("PromisesShim.WaitGetDate", "PromisesShim")]
    [return: JSMarshalAs<JSType.Promise<JSType.Date>>()]
    public static partial Task<DateTime> WaitGetDate();

    [JSImport("PromisesShim.FetchCurrentUrl", "PromisesShim")]    
    public static partial Task<string> FetchCurrentUrl();

    [JSImport("PromisesShim.AsyncFunction", "PromisesShim")]
    public static partial Task AsyncFunction();

    [JSImport("PromisesShim.ConditionalSuccess", "PromisesShim")]
    public static partial Task ConditionalSuccess(bool shouldSucceed);

}

public static class PromisesUsage
{
    public static async Task Run()
    {
        await JSHost.ImportAsync("PromisesShim", "/PromisesShim.js");
                
        Stopwatch sw = new Stopwatch();
        sw.Start();

        await PromisesProxy.Wait2Seconds();// await promise
        Console.WriteLine($"Waited {sw.Elapsed.TotalSeconds:#.0} seconds.");

        sw.Restart();
        string str = await PromisesProxy.WaitGetString();// await promise with string return
        Console.WriteLine($"Waited {sw.Elapsed.TotalSeconds:#.0} seconds for WaitGetString: '{str}'");

        sw.Restart();
        DateTime date = await PromisesProxy.WaitGetDate();// await promise with string return
        Console.WriteLine($"Waited {sw.Elapsed.TotalSeconds:#.0} seconds for WaitGetDate: '{date}'");

        string responseText = await PromisesProxy.FetchCurrentUrl();// await a JS fetch        
        Console.WriteLine($"responseText.Length: {responseText.Length}");
                
        sw.Restart();

        await PromisesProxy.AsyncFunction();// await an async JS method
        Console.WriteLine($"Waited {sw.Elapsed.TotalSeconds:#.0} seconds for AsyncFunction.");

        try {
            // Handle a promise rejection
            await PromisesProxy.ConditionalSuccess(shouldSucceed: false);// await an async JS method            
        }
        catch(JSException ex) // Catch JavaScript exception
        {
            Console.WriteLine($"JavaScript Exception Caught: '{ex.Message}'");
        }       
        
    }
    // The example displays the following output in the browser's debug console:
    // Waited 2.0 seconds.
    // Waited .5 seconds for WaitGetString: 'String From Resolve'
    // Waited .5 seconds for WaitGetDate: '11/24/1988 12:00:00 AM'
    // responseText.Length: 582
    // Waited 2.0 seconds for AsyncFunction.
    // JavaScript Exception Caught: 'Reject: ShouldSucceed == false'

}
```

### Conditional Promises

Methods that can return promises should consistently return a promise for all code paths.  If you have logic where certain circumstances do not need to execute awaitable code and should return immediately, then just return a resolved promise. This is necessary so that all code paths return some form of a promise for consistent handling by the caller:

```JS
HelpersProxy.LoadjQuery = function (url) {
    if (window.jQuery) {
        // jQuery already loaded and ready to go, nothing to await
        return Promise.resolve();// return a resolved promise
    } else {
        // returns promise that resolves when script is loaded
        return HelpersProxy.LoadScript(url);
    }
};
```

### Await Loading a Script Tag

While modules are the preferred way to load scripts, sometimes it is necessary to load a traditional script on-demand using a script tag.  The following example demonstrates a JS method that loads a script tag and returns a promise that resolves when the script is loaded.  The C# method can await the promise to ensure the script is loaded before continuing execution.

```JS
globalThis.LoadScript = function (url) {
    return new Promise(function (resolve, reject) {
        var script = document.createElement("script");
        script.onload = resolve;
        script.onerror = reject;
        script.src = url;
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
        LoadScript(string url);
}

// Usage:
await HelpersProxy.LoadScript("https://code.jquery.com/jquery-3.7.1.min.js");
```

## Subscribing to JS Events

.NET code can subscribe to and handle JS events by passing a C# Action to a JS method to act as a handler.  The JS shim code handles subscribing to the event.

One nuance of `removeEventListener()` is it requires a reference to the function previously passed to `addEventListener()`.  When a C# Action is passed across the interop boundary, it gets wrapped in a JS proxy object.  The consequence of this is when passing the same C# Action to both `addEventListener` and `removeEventListener`, two different JS proxy objects wrapping the Action will be generated.  These references are different, thus `removeEventListener` will not be able to find the event listener to remove.  To address this problem, the following examples wrap the C# Action in a JS function, then return that reference as a JSObject from the subscribe call to be passed later to the unsubscribe call.  Because it is returned and passed as a JSObject, the same reference is used for both calls, and the event listener can be removed.

```JS
// EventsShim.js
let EventsShim = {};
(function (EventsShim) {

    EventsShim.SubscribeEventById = function (elementId, eventName, listenerFunc) {
        const elementObj = document.getElementById(elementId);

        // Need to wrap the Managed C# action in JS func (only because it is being returned)
        let handler = function (event) {            
            listenerFunc(event.type, event.target.id);// decompose object to primitives
        }.bind(elementObj);        

        elementObj.addEventListener(eventName, handler, false);        
        return handler;// return JSObject reference so it can be used for removeEventListener later
    }

    // Param listenerHandler must be the JSObject reference returned from the prior SubscribeEvent call
    EventsShim.UnsubscribeEventById = function (elementId, eventName, listenerHandler) {
        const elementObj = document.getElementById(elementId);
        elementObj.removeEventListener(eventName, listenerHandler, false);
    }

    EventsShim.TriggerClick = function (elementId) {
        const elementObj = document.getElementById(elementId);
        elementObj.click();
    }


    EventsShim.GetElementById = function (elementId) {
        return document.getElementById(elementId);
    }

    EventsShim.SubscribeEvent = function (elementObj, eventName, listenerFunc) {
        // Need to wrap the Managed C# action in JS func
        let handler = function (e) {
            listenerFunc(e);
        }.bind(elementObj);

        elementObj.addEventListener(eventName, handler, false);
        return handler;// return JSObject reference so it can be used for removeEventListener later
    }

    EventsShim.UnsubscribeEvent = function (elementObj, eventName, listenerHandler) {        
        return elementObj.removeEventListener(eventName, listenerHandler, false);
    }
    
})(EventsShim);

export { EventsShim };
```

```C#
using System;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;

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
        await JSHost.ImportAsync("EventsShim", "/EventsShim.js");

        Action<string, string> listenerFunc = (eventName, elementId) =>
            Console.WriteLine($"In C# event listener: Event {eventName} from ID {elementId}");
        
        JSObject listenerHandler1 = EventsProxy.SubscriveEventById("btn1", "click", listenerFunc);
        JSObject listenerHandler2 = EventsProxy.SubscriveEventById("btn2", "click", listenerFunc);
        Console.WriteLine("Subscribed to btn1 & 2.");
        EventsProxy.TriggerClick("btn1");
        EventsProxy.TriggerClick("btn2");

        EventsProxy.UnsubscriveEventById("btn2", "click", listenerHandler2);
        Console.WriteLine("Unsubscribed btn2.");
        EventsProxy.TriggerClick("btn1");
        EventsProxy.TriggerClick("btn2");// Doesn't trigger because unsubscribed
        EventsProxy.UnsubscriveEventById("btn1", "click", listenerHandler1);
        // Pitfall: Using a different handler for unsubscribe will silently fail.
        // EventsProxy.UnsubscriveEventById("btn1", "click", listenerHandler2); 
        

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
```

### Interop with the Event Object

If the Event object is passed directly as a JSObject, then we can access properties of the event using JSObject's.GetPropertyAs\* methods.  For example, we might use `eventObj.GetPropertyAsJSObject("target")` to retrieve `event.target` as a JSObject reference to the HTMLElement, then pass this to other interop methods that might change the state of the element or retrieve data from other HTML elements such as a parent form.

Alternative approaches of interacting with the event object:
- Passing the event object and marshelling as a JSObject, then calling the JSObject's GetPropertyAs\*().
- Calling a JSImport'd method and passing the eventObj as a parameter, and allowing the JS shim to access or operate on the parameters.
- Wrapping our `listenerFunc` in the JS shim implementation to either fully or partially serialize the eventObj to a JSON string before passing it the C# event handler where it can be deserialized.  This may have undesirable side effects since serializing a property such as event.currentTarget will lose its reference as an HTMLElement.  
- Wrapping our `listenerFunc` in the JS shim implementation, extracting additional values from the eventObj or DOM, and passing them as primtive parameters to our event listener.  Requires our event listener be declared with additional parameters.  This is demonstrated in the JS previous `SubscribeEventById` method where `listenerFunc(event.type, event.target.id  )` passes to primitive values from the event to the C# listener.

SerratedJQ uses an advanced approach, where it partially serializes the event object, and uses a visitor pattern to insert replacement placeholders and preserve references to HTMLElement/jQueryObject references in an array.  An intermediate listener deserializes the JSON, and restores the JSObject references.  This hybrid approach allows most primitive values of the event to be accessed naturally without interop, while specific references such as target/currentTarget properties can be acted on as a JSObject.  This is required where we would want to interact with \*.currentTarget's HTMLElement through interop.

### Instance Methods as Event Listeners

An instance method can be used as an event listener:

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

For example, this can be valuable when implementing a wrapper in C# that encapsulates an HTMLElement or HTML fragment.  This is demonstrated in the SerratedJQSample project's [ProductSaleRow](https://github.com/SerratedSharp/SerratedJQ/blob/d2406a1b94334f6fc3ceba422e74f25d289004bb/SerratedJQSample/Sample.Wasm/ClientSideModels/ProductSaleRow.cs#L28) which wraps an HTML fragment as a component, proxies JS events, and includes backing model data in the event specific to that row/instance.  This allows downstream consumers of the component to subscribe to a strongly typed event which includes the model data, and hides the complexities of JS interop from the caller.

### Wrapping a JS Event as a C# Event

JS events can be exposed as classic .NET/C# events.  This presents an event handling implementation more familiar to .NET as shown by this usage example:

```C#
Element.JSObjectWrapperEventHandler<Element, JSObject> managedHandler = 
    (Element sender, JSObject e) => {
        Console.WriteLine($"[Strongly Typed Event Handler] Event fired through C# event '{e.GetPropertyAsString("type")}'");
        // Log sender and event object
        PrimitivesProxy.ConsoleLog(sender.JSObject);
        PrimitivesProxy.ConsoleLog(e);
    };

element.OnClick += managedHandler;// Subscribe to the event
EventsProxy.TriggerClick("btn3");// Trigger event to test hander
element.OnClick -= managedHandler;// Unsubscribe from the event
```

The same JS shim `subscribeEvent` and `unsubscribeEvent` methods and proxies implemented [previously](#Subscribing-to-JS-Events) are leveraged.

We'll use a wrapper for HTML Element to demonstrate an implementation of the event, but any client side component you wish to use an event handling pattern for could leverage this approach.  For example, notifications could be published as a C# event.

To minimize the amount of interop occurring for this event, we support multiple C# subscribers through a single JS interop event subscription.

```C#
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
```


```C#
// Usage example
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
        element.OnClick += test;// Subscribe to the event
        EventsProxy.TriggerClick("btn3");// trigger event to test hander
        Console.WriteLine("-= test");
        element.OnClick -= test;// Unsubscribe from the event
        EventsProxy.TriggerClick("btn3");// trigger event again to verify event no longer fired
        Console.WriteLine("+= test");
        element.OnClick += test;
        EventsProxy.TriggerClick("btn3");
    }
}
```

See the complete [Strongly Typed Event Example](https://github.com/SerratedSharp/CSharpWasmRecipes/blob/main/WasmBrowser.Recipes.WasmClient/Examples/StronglyTypedEvents.cs)

> [!Note]
> If your goal is to subscribe specifically to HTML DOM events, SerratedJQ demonstrates a more flexible approach with `JQueryPlainObject.OnClick` and `.On(eventName, ...)` to support this pattern across a greater range of events without the need for the above boiler plate code, but relies on JQuery for the internal implementation.

### Triggering JS Events from C#

As demonstrated previously, we can trigger JS events from .NET.  This is useful for testing event handlers, writing UI unit tests, or for ensuring that a programmatically driven action follows the same code path as the event handler.  For example, triggering a click event on a button element:

Declare JS shim:
```JS
globalThis.click = function(elementObj) { return elementObj.click(); }    
```

```C#
public partial class EventsProxy {
    [JSImport("globalThis.click")]
    public static partial string Click(JSObject elementObj);
}

//Usage:
EventsProxy.Click(element); // Trigger the click event
```

## Instance Wrappers

To make calling instance methods on a JS object more natural, a wrapper class can be created to hold a reference to the JSObject and expose instance methods.  The wrapper class holds the JSObject reference internally.

Handling of JSObject's must be done with care, as there is no strong typing to prevent a reference to the wrong underlying type from being used incorrectly.  For this reason, these wrappes encapsulate handling of JSObject's, to guarantee APIs returning JS objects of a certain underlying type are wrapped with the corresponding .NET type.

We begin with the usual JS shim and C# proxy.  Note we have two nested namespaces, `Document` and `Element`, for the sake of organizing code relevent to each proxy and object.

```JS

let DomShim = globalThis.DomShim || {};// Conditionally create namespace
(function (DomShim) {

    let Element = DomShim.Element || {};// create child namespace
    DomShim.Element = Element; // add to parent namespace

    let Document = DomShim.Document || {};// create child namespace
    DomShim.Document = Document; // add to parent namespace

    Element.GetAttribute = (elementObj, attributeName) =>
        elementObj.getAttribute(attributeName);

    Element.SetAttribute = (elementObj, attributeName, attributeValue) =>
        elementObj.setAttribute(attributeName, attributeValue);

    Element.AppendHtml = (elementObj, htmlString) =>
        elementObj.innerHTML += htmlString;

    Document.GetElementById = (elementId) =>
        globalThis.document.getElementById(elementId);

})(DomShim);

export { DomShim }; 

```

```C#
using System.Runtime.InteropServices.JavaScript;

// Proxy that would hold any declarations if they existed at the root of the JS module.
// Also declares the module name.
internal partial class DomShimModule
{
    public const string ModuleName = "DomShim";
}

// Optionally make *Proxy classes internal in a class library so only the wrapper can see them.
// Or nest this as a Private class inside the wrapper class.
internal partial class ElementProxy
{
    // See wwwroot/DomShim.js for implementation details.
    private const string moduleName = DomShimModule.ModuleName;
    private const string baseJSNamespace = $"{moduleName}.Element";

    [JSImport($"{baseJSNamespace}.{nameof(GetAttribute)}", moduleName)]
    public static partial string GetAttribute(JSObject jSObject, string attributeName);

    [JSImport($"{baseJSNamespace}.{nameof(SetAttribute)}", moduleName)]
    public static partial void SetAttribute(JSObject jSObject, string attributeName, string value);

    [JSImport($"{baseJSNamespace}.{nameof(AppendHtml)}", moduleName)]
    public static partial void AppendHtml(JSObject jSObject, string html);
}

internal partial class DocumentProxy
{
    private const string moduleName = DomShimModule.ModuleName;
    private const string baseJSNamespace = $"{moduleName}.Document";

    [JSImport($"{baseJSNamespace}.{nameof(GetElementById)}", moduleName)]
    public static partial JSObject GetElementById(string id);
}
```

For example, `Document`'s static GetElementById retrieves a JSObject representing a JS Element, and then creates a new wrapper Element for the JSObject.  So rather than constructing a JS instance directly, we leverage existing JS APIs to retrieve objects, then use Element's constructor to wrap the JSObject.

Downstream developers consuming this API would only interact with the `Document` and `Element` classes, and would not need to be aware of the underlying JSObject's nor `*Proxy` classes.

```C#
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;

public partial class Document
{
    public static Element GetElementById(string id)
    {
        JSObject jsObject = DocumentProxy.GetElementById(id);
        return new Element(jsObject);// Wrap the JSObject in our Element wrapper.
    }
}

// Instance wrapper for JS Element
public partial class Element
{
    internal JSObject jsObject;// reference to the JavaScript interop object

    /// <summary>
    /// Handle to the underlying JavaScript object.
    /// </summary>
    public JSObject JSObject => jsObject;

    // Internal/private constructor. Wrappers handle the two step process of retrieving and wrapping JSOBjects.
    private Element() { }

    // Copy constructor to allow wrapping of existing JSObject instances.  Consider making this internal unless you want to allow consumers to wrap arbitrary JSObjects directly.
    internal Element(JSObject jsObject) { this.jsObject = jsObject; }

    public string GetAttribute(string attributeName)
        => ElementProxy.GetAttribute(jsObject, attributeName);

    public void SetAttribute(string attributeName, string value)
        => ElementProxy.SetAttribute(jsObject, attributeName, value);
    
    public void AppendHtml(string html) 
        => ElementProxy.AppendHtml(jsObject, html);
}
```


Consumers of the wrapper classes would interact with the strongly typed wrappers `Document` and `Element` as demonstrated below.  The `Document` class retrieves a JSObject representing a JS Element, and then returns an `Element` wrapper.  The `Element` class exposes instance methods that interact with the underlying JS object.  In this example, we leverage this to interact with the HTML DOM, but this approach can be used to wrap any JS object instance:

```C#
public static class StronglyTypedWrapperUsage
{
    public static async Task Run()
    {
        await JSHost.ImportAsync(DomShimModule.ModuleName, $"/{DomShimModule.ModuleName}.js");

        Element container = Document.GetElementById("testContainer");
        container.AppendHtml("<h1 id='header'>New Hello!</h1>");

        Document.GetElementById("header")
            .SetAttribute("style", "color: red;");
    }
}
// The example displays a red "New Hello!" element in the browser.
```

In a class library, the `DocumentProxy` and `ElementProxy` classes would be marked internal so that all consuming access goes through strongly typed wrappers `Document` and `Element`, and also avoids confusion of additional types being exposed that should not be used.

Optionally if not using a class library, for example if the wrappers and proxies are declared in the same project that is consuming the wrappers, then the proxy class declarations can be nested inside the wrappers and marked private:

```C#
public partial class Element
{
    internal JSObject jsObject;
    public JSObject JSObject => jsObject;
    
    private Element() { }

    public Element(JSObject jsObject) { this.jsObject = jsObject; }

    public string GetAttribute(string attributeName) => ElementProxy.GetAttribute(jsObject, attributeName);
    public void SetAttribute(string attributeName, string value) => ElementProxy.SetAttribute(jsObject, attributeName, value);
    public void AppendHtml(string html) => ElementProxy.AppendHtml(jsObject, html);

    private partial class ElementProxy 
    {
        private const string moduleName = DomShimModule.ModuleName;
        private const string baseJSNamespace = $"{moduleName}.Element";

        [JSImport($"{baseJSNamespace}.{nameof(GetAttribute)}", moduleName)]
        public static partial string GetAttribute(JSObject jSObject, string attributeName);

        [JSImport($"{baseJSNamespace}.{nameof(SetAttribute)}", moduleName)]
        public static partial void SetAttribute(JSObject jSObject, string attributeName, string value);

        [JSImport($"{baseJSNamespace}.{nameof(AppendHtml)}", moduleName)]
        public static partial void AppendHtml(JSObject jSObject, string html);
    }
}
```

Using a class library and `internal` access modifier is preferrable because it allows wrappers to call across multiple complimentary proxy classes as needed.   For example, `Document` methods could still access internal methods of `ElementProxy` or vice versa if needed.  Private nested classes prevent this usage.

We may opt to keep the `Element` constructor public in cases where we may not fully wrap the entire surface area of a JS API.  For example, the consuming dev may implement their own interop to supplement our API, in which case may be retrieving JSObject's.  If they want to benefit from our wrapper, they would want to use the strongly typed wrapper's `Element(JSObject)` constructor to wrap their JSObject.  If making this constructor public, consider interop validation in the constructor to verify the underlying JS type of the JSObject and throw an exception if invalid:

```JS
ElementNS.IsElement = (obj) => obj instanceof Element;
```

```C#
public partial class Element 
{
    public Element(JSObject jsObject) 
    {
        if ( !ElementProxy.IsElement(jsObject) )
            throw new ArgumentException("JSObject parameter does not refer to a JS Object of type Element.");

        this.jsObject = jsObject; 
    }
    //...
}
```

## Proxyless Instance Wrappers

Proxying calls to instance methods through two layers of C# and JS static methods requires a cumbersome amount of boilerplate code.  In some cases, it may be desirable to create a wrapper for a JS instance methods without corresponding proxy methods.  This can be achieved by leveraging the SerratedSharp.JSInteropHelpers Nuget package.  The below example demonstrates the prior wrapper using this library.

This approach is useful when creating wrappers for existing JS instances/classes.  While proxies are still needed for static methods and those with custom implementations, eliminating instance method proxies will reduce the amount of boilerplate code needed which can be significant for larger API surface areas.  For example, the SerratedJQ library implements the vast majority of the JQuery API surface area with this approach, thus eliminating the need for a significant amount of JS shim code.  

This approach depends on `SerratedSharp.JSInteropHelpers`. Internally, this library leverages this JavaScript shim to support dynamically calling a function with any given set of parameters:

```JS
    HelpersProxy.FuncByNameToObject = function (jsObject, funcName, params) {
        const rtn = jsObject[funcName].apply(jsObject, params);
        return rtn;
    };
```

The code from the prior [Instance Wrappers](#instance-wrappers) example is updated to remove instance methods which call a method of the same name:

```JS

let ProxylessDomShim = globalThis.ProxylessDomShim || {};// Conditionally create namespace
(function (ProxylessDomShim) {

    let Element = ProxylessDomShim.Element || {};// create child namespace
    ProxylessDomShim.Element = Element; // add to parent namespace

    let Document = ProxylessDomShim.Document || {};// create child namespace
    ProxylessDomShim.Document = Document; // add to parent namespace

    // Instance methods no longer needed in the proxyless scenario.
    //Element.GetAttribute = (elementObj, attributeName) =>
    //    elementObj.getAttribute(attributeName);

    //Element.SetAttribute = (elementObj, attributeName, attributeValue) =>
    //    elementObj.setAttribute(attributeName, attributeValue);

    // Still needed to proxy since implementation isn't a method call.
    Element.AppendHtml = (elementObj, htmlString) => {               
        elementObj.innerHTML += htmlString;
    }

    Document.GetElementById = (elementId) =>
        globalThis.document.getElementById(elementId);

})(ProxylessDomShim);

export { ProxylessDomShim }; 
```

The proxy can also omit these instance methods:

```C#
using System;
using System.Runtime.InteropServices.JavaScript;

internal partial class ElementProxy
{
    // See wwwroot/DomShim.js for implementation details.
    private const string moduleName = ProxylessDomShimModule.ModuleName;
    private const string baseJSNamespace = $"{moduleName}.Element";
    // No longer needed due to use of CallJSOfSameName from SerratedSharp.JSInteropHelpers
    //    [JSImport($"{baseJSNamespace}.{nameof(GetAttribute)}", moduleName)]
    //    public static partial string GetAttribute(JSObject jSObject, string attributeName);

    //    [JSImport($"{baseJSNamespace}.{nameof(SetAttribute)}", moduleName)]
    //    public static partial void SetAttribute(JSObject jSObject, string attributeName, string value);

    [JSImport($"{baseJSNamespace}.{nameof(AppendHtml)}", moduleName)]
    public static partial void AppendHtml(JSObject jSObject, string html);

}

internal partial class DocumentProxy
{
    private const string moduleName = ProxylessDomShimModule.ModuleName;
    private const string baseJSNamespace = $"{moduleName}.Document";

    [JSImport($"{baseJSNamespace}.{nameof(GetElementById)}", moduleName)]
    public static partial JSObject GetElementById(string id);
}

public static class ProxylessWrapperUsage
{
    public static async Task Run()
    {
        await JSHost.ImportAsync(ProxylessDomShimModule.ModuleName, $"/{ProxylessDomShimModule.ModuleName}.js");

        var container = Document.GetElementById("testContainer");
        container.AppendHtml("<h1 id='header2'>Proxyless Wrapper Hello!</h1>");
        Document.GetElementById("header2")
            .SetAttribute("style", "color: blue;");
    }
}
// The example displays a blue "Proxyless Wrapper Hello!" element in the browser.
```

The wrapper implements the IJSObjectWrapper interface from SerratedSharp.JSInteropHelpers.  This interface enables the extension methods `CallJSOfSameName<TReturn>` and `CallJSOfSameNameAsWrapped`, which call instance methods on the underlying JSObject.  These methods operate by convention, using the name of the C# method to determine the name of the underlying JS method(using `[CallerMemberName]`).  The generic parameter indicates the expected return type of the JS method.  The CallJSOfSameNameAsWrapped method indicates the expected return type is the same as the wrapped class, and will handle wrapping the JSObject by calling your implementation of `IJSObjectWrapper<Element>.WrapInstance`.

```C#
using System;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
using SerratedSharp.JSInteropHelpers;

public partial class Document
{
    public static Element GetElementById(string id)
    {
        JSObject jsObject = DocumentProxy.GetElementById(id);
        return new Element(jsObject);// Wrap the JSObject in our Element wrapper.
    }
}

public partial class Element : IJSObjectWrapper<Element>
{
    internal JSObject jsObject;
    public JSObject JSObject => jsObject;
    
    private Element() { } 
    public Element(JSObject jsObject) { this.jsObject = jsObject; }

    // Static factory method required by IJSObjectWrapper enables generic code such as CallJSOfSameNameAsWrapped to automatically wrap JSObjects
    static Element IJSObjectWrapper<Element>.WrapInstance(JSObject jsObject) 
        => new Element(jsObject);
    
    public string GetAttribute(string attributeName)
        // will call JSObject's `.getAttribute(attributeName)`, derived from the parent method name `GetAttribute`.
       => this.CallJSOfSameName<string>(attributeName);

    public void SetAttribute(string attributeName, string value)
       => this.CallJSOfSameName<object>(attributeName, value);

    public void AppendHtml(string html)
       => ElementProxy.AppendHtml(this.JSObject, html);
}

internal partial class ProxylessDomShimModule
{
    public const string ModuleName = "ProxylessDomShim"; // declares module name and JS filename
}
```

> [!Important]
> Some edge cases require careful handling of parameters to achieve desired results.  While I have thoroughly tested SerrateJQ which internally leverages `SerratedSharp.JSInteropHelpers`, there are some rough spots in JSInteropHelpers that could be pitfalls for developers, as I have not refinemed the API for broader usage.  The implementation of [JQueryPlainObject](https://github.com/SerratedSharp/SerratedJQ/blob/main/SerratedJQLibrary/SerratedJQ/Plain/JQueryPlainObject.cs) thoroughly exercises JSInteropHelpers and demonstates handling of some edge cases.  I'd be happy for feedback if others are interested in using this library.

## Proxyless Instance Methods on Global Objects

We can use `Lazy<>` to retieve a handle to a global JS object such as `console` and access its instance methods without needing to implement a shim nor proxy.  This ensures:

- We don't generate an unnecesary interop call if the object is never accessed.
- We don't need to implement a JS shim nor explicitly map JSImport's on a proxy.
- We don't access the global object until the first time it is accessed, which can be important if the object is not yet defined when the application starts.
- Subsequent accesses use the existing handle without re-evaluating the Lazy accessor.

To implement access to instance methods on the object, we can use the `CallJSOfSameName` extension method from `SerratedSharp.JSInteropHelpers`.  This method will call the instance method on the JS object with the same name as the C# method, and will pass the parameters to the JS method.  The generic parameter indicates the expected return type of the JS method.  For void return types `object` can be used and the return discarded.

```C#
using System.Runtime.InteropServices.JavaScript;
using SerratedSharp.JSInteropHelpers;

public static class JSConsole
{
    static Lazy<JSObject> _console = new(() => JSHost.GlobalThis.GetPropertyAsJSObject("console"));

    public static void Log(params object[] parameters)
    {   
        _console.Value.CallJSOfSameName<object>(parameters);
    }
}
```

If you wish for the C# method's name to differ, then optional parameters of CallJSOfSameNAme allows you to explicitely specify the JS method name.

# Performance Considerations for Interop

Marshalling of calls and the overhead of tracking objects across the interop boundary is more expensive than native .NET operations, but for moderate usage should still demonstrate acceptable performance for a typical web application.

Object proxies such as JSObject which maintain references across the interop boundary have additional memory overhead, and impact how garbage collection affects these objects.  Additionally, since memory pressure from JavaScript and .NET is not shared, it is possible in some scenarios to exhaust available memory without a garbage collection being triggered.  This risk is significant when an excessive number of large objects are referenced across interop by relatively small JSObject's, or vice versa where large .NET objects are referenced by JS proxies.  In such cases it is advisable to follow deterministic disposal patterns with `using` scopes leveraging JSObject's `IDisposable` interface.

The below benchmarks (leveraging prior example code) demonstrate that interop operations are roughly an order of magnitude slower than those that remain within the .NET boundary, but are still relatively fast.  Additionally, consider that a user's device capabilities will impact performance.

```C#
using System;
using System.Diagnostics;

public static class JSObjectBenchmark
{
    public static void Run()
    {
        Stopwatch sw = new Stopwatch();
        var jsObject = JSObjectProxy.CreateObject();
        sw.Start();
        for (int i = 0; i < 1000000; i++)
        {
            JSObjectProxy.IncrementAnswer(jsObject);
        }
        sw.Stop();
        Console.WriteLine($"JS interop elapsed time: {sw.Elapsed.TotalSeconds:#.0000} seconds at {sw.Elapsed.TotalMilliseconds / 1000000d:#.000000} ms per operation");

        var pocoObject = new PocoObject { Question = "What is the answer?", Answer = 41 };
        sw.Restart();
        for (int i = 0; i < 1000000; i++)
        {
            pocoObject.IncrementAnswer();
        }
        sw.Stop();
        Console.WriteLine($".NET elapsed time: {sw.Elapsed.TotalSeconds:#.0000} seconds at {sw.Elapsed.TotalMilliseconds / 1000000d:#.000000} ms per operation");

        Console.WriteLine($"Begin Object Creation");

        sw.Restart();
        for (int i = 0; i < 1000000; i++)
        {
            var jsObject2 = JSObjectProxy.CreateObject();
            JSObjectProxy.IncrementAnswer(jsObject2);
        }
        sw.Stop();
        Console.WriteLine($"JS interop elapsed time: {sw.Elapsed.TotalSeconds:#.0000} seconds at {sw.Elapsed.TotalMilliseconds / 1000000d:#.000000} ms per operation");

        sw.Restart();
        for (int i = 0; i < 1000000; i++)
        {
            var pocoObject2 = new PocoObject { Question = "What is the answer?", Answer = 0 };
            pocoObject2.IncrementAnswer();
        }
        sw.Stop();
        Console.WriteLine($".NET elapsed time: {sw.Elapsed.TotalSeconds:#.0000} seconds at {sw.Elapsed.TotalMilliseconds / 1000000d:#.000000} ms per operation");
    }
    
    public class PocoObject // Plain old CLR object
    {
        public string Question { get; set; }
        public int Answer { get; set; }

        public void IncrementAnswer() => Answer += 1;        
    }
}
// The example displays the following output in the browser's debug console:
// JS interop elapsed time: .2536 seconds at .000254 ms per operation
// .NET elapsed time: .0210 seconds at .000021 ms per operation
// Begin Object Creation
// JS interop elapsed time: 2.1686 seconds at .002169 ms per operation
// .NET elapsed time: .1089 seconds at .000109 ms per operation
```

# Troubleshooting

As of .NET 8, JavaScript interop is implemented with "good enough" error handling.  Appropriate errors are generated, but messages may not necessarily be intuitive nor informative.  Errors will typically appear in the browser console, and could potentially also bubble as an exception in .NET code.  This can vary depending on the executing WebAssembly platform.

All of the WebAssembly project types mentioned support integrated debugging in VisualStudio in one form or another.  Setting a breakpoint in the .NET code for the WebAssembly, launching in debug mode, and loading or refreshing the page should trigger execution of the WebAssembly's entry point and pausing on the breakpoint when reached.  Note some project settings can prevent debugging.

Debugging synchronization is supported through WebSocket communication between the browser and Visual Studio per the `inspectUri` setting in launchSettings.js.  In some cases you may observe execution pausing within the browser's debug console first.  In such a case, typically you would not choose the Continue or Resume option from within the browser, but instead wait a few seconds for synchonization, then the breakpoint in VisualStudio should then become highlighted.

Some failures will simply generate an error logged as an object `[object Object]` to the browser console.

Unfortunately some errors are not fail-fast, and are result of proceeding incorrect code.  For example, calling `JSHost.ImportAsync()` with an invalid moduleName parameter will succeed, but later attempts to call methods with a JSImport dependent on the module will fail.  Thus, the location where an error originates may be misleading.


## Type Mapping Limitations

Some type mappings requiring nested generic types in the `JSMarshalAs` definition are not currently supported.  For example, returning a JS promise for an array such as `[return: JSMarshalAs<JSType.Promise<JSType.Array<JSType.Number>>>()]` will generate a compile time error.  An appropriate workaround will vary depending on the scenario, but one option is to represent the array as a JSObject reference.  This may be sufficient if accessing individual elements within .NET is not necessary, and the reference can be passed to other JS methods which act on the array.  Alternatively, a dedicated method can take the JSObject reference as a parameter and return the materialized array as demonstrated by UnwrapJSObjectAsIntArray.  Note in this case the JS method has no type checking, and it's the developer's responsibility to ensure a JSObject wrapping the appropriate array type is passed.

```JS
    PromisesShim.WaitGetIntArrayAsObject = function () {
        return new Promise((resolve, reject) => {
            setTimeout(() => {
                resolve([1, 2, 3, 4, 5] ); // return an array from the Promise
            }, 500);
        });
    };

    PromisesShim.UnwrapJSObjectAsIntArray = function (jsObject) {
        return jsObject;
    };
```

```C#
    // Not supported, generates compile time error.
    [JSImport("PromisesShim.WaitGetArray", "PromisesShim")]
    [return: JSMarshalAs<JSType.Promise<JSType.Array<JSType.Number>>>()]
    public static partial Task<int[]> WaitGetIntArray();

    // Workaround, take the return from this call and pass it to UnwrapJSObjectAsIntArray
    // Return a JSObject reference to a JS number array
    [JSImport("PromisesShim.WaitGetArray", "PromisesShim")]
    [return: JSMarshalAs<JSType.Promise<JSType.Object>>()]
    public static partial Task<JSObject> WaitGetIntArrayAsObject();

    // Takes a JSOBject reference to a JS number array, and returns the array as a C# int array.
    [JSImport("PromisesShim.WaitGetArray", "PromisesShim")]
    [return: JSMarshalAs<JSType.Array<JSType.Number>>()]
    public static partial int[] UnwrapJSObjectAsIntArray(JSObject intArray);
    //...

    // Usage from Program.cs Main():
    JSObject arrayAsJSObject = await PromisesProxy.WaitGetIntArrayAsObject();          
    int[] intArray = PromisesProxy.UnwrapJSObjectAsIntArray(arrayAsJSObject);
```



## "JSObject proxy of ManagedObject proxy is not supported."
   
When a .NET object is passed to JS, then it gets wrapped in a managed proxy.  Conversely when a JS object is returned/passed to .NET, then it gets wrapped in a JSObject proxy.  However, if a .NET object is passed to JS, then returned to .NET, it will be wrapped twice.  Once in managed proxy, and then again in a JSObject proxy.  This will generate the error "JSObject proxy of ManagedObject proxy is not supported.".

```JS  
    EventsShim.SubscribeEventFailure = function (elementObj, eventName, listenerFunc) 
    {
        // It's not strictly required to wrap the C# action listenerFunc in a JS function.
        elementObj.addEventListener(eventName, listenerFunc, false);
        // However, if you need to return the wrapped proxy object you will get an error when it tries to wrap the existing proxy in an additional proxy:
        return listenerFunc; // Error: "JSObject proxy of ManagedObject proxy is not supported."
    }
```

Depending on the goal, one workaround is to wrap the .NET object in a JS object yourself, so that when returned the marshalling will be wrapping a native JS object.

```C#
    EventsShim.SubscribeEvent = function (elementObj, eventName, listenerFunc) 
    {
        // Need to wrap the Managed C# action in JS func (only because it is being returned)
        let handler = function (event) {
            console.log(event);
            listenerFunc(event.type, event.target.id);// decompose object to primitives
        }.bind(elementObj);

        elementObj.addEventListener(eventName, handler, false);
        
        return handler;// return JSObject reference so it can be used for removeEventListener later
    }
```

## Debugging with Logging

Sometimes the debugging experience with interop can be challenging. Errors may originate from framework code with no indication of any user code among the stack trace, and/or errors may be very vague.

In such cases, adding logging can be helpful to narrow down the source of an error.  This can be done in JS shims and/or C# code.

```JS      
    EventsShim.SubscribeEventByIdWithLogging = function (elementId, eventName, listenerFunc) {
       
        const elementObj = document.getElementById(elementId);
        console.log("elementObj:", elementObj);
        // Need to wrap the Managed C# action in JS func (only because it is being returned)
        let handler = function (event) {
            console.log(event);
            listenerFunc(event.type, event.target.id);// decompose object to primitives
        }.bind(elementObj);
        console.log(handler);

        elementObj.addEventListener(eventName, handler, false);
        console.log("done");
        return handler;// return JSObject reference so it can be used for removeEventListener later
    }
```
