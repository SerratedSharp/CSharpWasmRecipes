
# Architecture

## Runtime Dependencies

WebAssembly/WASM support is dependent upon the browser's support for the WebAssembly standard, and is currently supported by the newest version of all major browsers: https://caniuse.com/?search=WebAssembly

Otherwise at runtime the resulting WASM package is platform agnostic, and there is no external dependency on specific hosting technologies, web application technologies, or programming languages.  At runtime, any HTML page that can load and execute the embedded.js bootstrapper will be able to successfully load and execute the WASM package.

## Interop

WASM interfaces with the web application or HTML page client side through javascript, known as JS interop, and/or via traditional web requests against the backend host such as REST.  Using native .NET 7 JS interop, static JS methods can be called from .NET.  Arbitrary JS cannot be executed via this approach, thus typical implementations require a JS implementation to act as a shim for the .NET interop.  In turn, JSExport'd methods can be accessed from an existing web application's client side JS as though they were static methods exposed from a traditional JS library.  WASM also supports JS promises and events offering additional integration options.  

The WASM package does not have direct access to all browser capabilities, but JS shims can be used to access those capabilities indirectly.  For example, access to the HTML DOM can be implemented by JSImport'ing JS shims calling native JS methods such as findElementById or creating a wrapper around a library such as jQuery as is done in SerratedJQ.

.NET 7 WASM supports HttpClient, allowing requests to be made directly to the backend host using traditional web requests that would be compatible with any hosting technology exposing traditional HTTP endpoints.  This could be used to retrieve HTML fragments, or JSON data models, either of which could be used to data driven logic or dynamically updating HTML.

The Uno WebAssemblyRuntime library provides methods to execute arbitrary javascript from .NET, but this should generally only be used for static JS that is not parameterized nor dynamic, due to security risks of dynamic JS.  It can be useful for creating JS declarations for the interop shims since these consist of static JS.

## Hosting

At runtime, the WASM package files will be downloaded from the server the same way static files such as images or JS would be downloaded, and then executed within the browser.  This is completely hosting platform agnostic, since from the host's perspective it is a simple file download request.  Often the host will need to be configured to allow files with *.clr and *.dat extensions to be downloaded, typically accomplished by adding mime types.

## Loading the WASM Package at Runtime

The HTML pages will need to reference the appropriate javascript to load the WASM packages.  Javascript files included by Uno.Bootstrap handle the initial loading of the runtime.  For example, if using EmbeddedMode, then a script tag referencing the WASM package's `embedded.js` would handle bootstrapping the WASM package, then execute our WASM entry point which is the console project's `Program.Main()`.

The WASM package can either be hosted from the same site as the primary application, or in a separate application.  Similar to any other javascript, it can be loaded from a relative URL (hosted in a subpath of the web app) or from another site.

## Solution Structure

### Single Page Application

- Solution
  - \*.WasmClient Console Project     
    - References Uno.Bootstrap.Wasm
    - In Dev/Debug environment, references Uno.Wasm.Bootstrap.DevServer to act as the static file host and support the debugging connection.
    - Hosts and serves its own index.html, which automatically includes and executes necessary javascript to load and execute the WASM package.
    - Optionally communicates with backend or remote APIs via HttpClient or ClientWebSocket using HTTP requests or WebSocket connections.  Allowing communication with backend APIs that may or may not necessarily be .NET hosts.   
 
This application could be built and deployed to any host supporting static files, which means a .NET host or backend is not required.

Without a backend API, such an application might still be useful as some sort of calculator that runs completely client side.  [C# WASM JQuery Demo](https://serratedsharp.github.io/CSharpWasmJQueryDemo/) is a trivial example of an application with no backend host other than being served as static files from github.io, running client side in the browser, with no backend API communication.

### ASP.NET (MVC) Hosted Application

- Solution
  - \*.WasmClient Console Project 
    - References Uno.Bootstrap.Wasm
    - In Dev/Debug environment, references Uno.Wasm.Bootstrap.DevServer    
    - Does **not** serve its index.html page.
    - Communicates with the ASP.NET backend or other remote APIs via HttpClient or ClientWebSocket using HTTP requests or WebSocket connections. 
  - ASP.NET (MVC) Project
    - In Dev/Debug environment, loads the WasmClient via `<script src="https://localhost:11111/embedded.js"></script>` where :11111 is the port the WasmClient's Uno.Bootstrap.DevServer is listening on.  This is found in WasmClient's launcSettings.json applicaitonUrl.
    - In Test/Production environment, loads the WasmClient via `<script src="embedded.js"></script>` where the path is relative to the ASP.NET project's wwwroot folder and the WASM package's dist files have been copied at publish/deployment time into the ASP.NET's wwwroot.
    - Exposes API endpoints that the WasmClient calls to perform actions, return data models, or return HTML fragments (partial view).

### Optional Projects

- WebAPI Project (optional)
    - Exposes API endpoints that the WasmClient calls to retrieve data models, HTML fragments, or perform operations.
- \*.WasmShared Class Library Project (optional)
    - Contains shared code that is referenced by both the WasmClient and WebAPI projects.  
    - Typically contains client API data models, client/server side dual validations, and other code that is used by both the client and server.
    - Reduces duplication of code that would often be replicated in both C# and Javascript.
    - Creates a clear delineation of what code is included in the client side package.

        

## Debugging

There is a WASM debugger available within Chrome DevTools which is covered in Uno Bootstrap's [Using the browser debugger](https://platform.uno/docs/articles/debugging-wasm.html#using-the-browser-debugger) documentation.

Debugging integration is also supported in Visual Studio 2022 with an experience more familiar to .NET developers.  This supports breakpoints within the WASM project's C# code, stepping through code, and inspecting values.  The following has been verified in VS 2022 >=17.8.

In the local development environment a browser link websocket is used to communicate breakpoint and line number information between the browser where the code is executing and the Uno.Bootstrap.DevServer webhost, which in turn drives the debugging experiene in Visual Studio.  Only one connection at a time will work, so launching multiple browsers will cause all but the first to fail to connect the for debugging.  In both of the following scenarios, the inspectUrl connects to the WasmClient's Uno.Bootstrap.DevServer host since only that host has the capability to communicate debugging information.


### Debugging Config for Single Page Application

The Properties/launchSettings.json file of the WASM console project would include an inspectUri that defines the webSocket that the browser will connect to for communicating debugging information:

{
  "profiles": {
    "Sample.WasmClient": {
      "commandName": "Project",
      "launchBrowser": true,
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      },
      "applicationUrl": "https://localhost:50044;http://localhost:50045",
      "inspectUri": "{wsProtocol}://{url.hostname}:{url.port}/_framework/debug/ws-proxy?browser={browserInspectUri}"
    }
  }
}

When the WasmClient is launched, a browser is launched connecting to the index.html, and javascript will initiate a websocket connection to the inspectUri hosted by the Uno.Bootstrap.DevServer.

For embdedded mode the inspectUri is instead set in the ASP.NET application, and points to the host:port of the WasmClient's Uno.Bootsrap.DevServer host.



