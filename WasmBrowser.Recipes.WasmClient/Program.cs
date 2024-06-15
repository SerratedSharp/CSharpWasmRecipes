using System;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
using WasmBrowser.Recipes.WasmClient.Examples;

namespace WasmBrowser.Recipes.WasmClient;

internal class Program
{
    public static string BaseUrl { get; set; }

    private static async Task Main(string[] args)
    {
        Console.WriteLine("Hello, Browser!");

        // Get base URL
        // Note, this requires multiple interop calls, where as creating a JS shim to handle this in a single call would be more efficient.
        BaseUrl = JSHost.GlobalThis.GetPropertyAsJSObject("window").GetPropertyAsJSObject("location").GetPropertyAsString("href");
        Console.WriteLine($"Base URL: {BaseUrl}");

        // Note: Many examples print to the browser's debug console to demonstrate results.
        await PrimitivesUsage.Run();
        await DateUsage.Run();
        await JSObjectUsage.Run();
        await PromisesUsage.Run();
        await EventsUsage.Run();
        await Examples.StronglyTypedWrapper.StronglyTypedWrapperUsage.Run();
        await Examples.StronglyTypedEvents.StronglyTypedEventsUsage.Run();

        // Load JSInteropHelpers dependencies
        await SerratedSharp.SerratedJQ.JSDeclarations.LoadScriptsForWasmBrowser();
        
        await Examples.ProxylessWrapper.ProxylessWrapperUsage.Run();
        
        await JSObjectBenchmark.Run();
        
    }

}
