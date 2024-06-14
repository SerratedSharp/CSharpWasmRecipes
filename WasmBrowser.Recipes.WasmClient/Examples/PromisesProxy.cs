using System;
using System.Diagnostics;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;

namespace WasmBrowser.Recipes.WasmClient.Examples;

// See wwwroot/PromisesShim.js for implementation details.
public partial class PromisesProxy
{
    // Do not use the async keyword in the C# method signature. Returning Task or Task<T> is sufficient.

    // When calling asynchrounous JS methods, we often want to wait until the JS method completes execution.
    // For example, if loading a resource or making a request, we likely want the following code to be able to assume the action is completed.

    // If the JS method or shim returns a promise, then C# can treat it as an awaitable Task.

    // For a promise with void type, declare a Task return type:
    [JSImport("PromisesShim.Wait2Seconds", "PromisesShim")]
    public static partial Task Wait2Seconds();

    [JSImport("PromisesShim.WaitGetString", "PromisesShim")]
    public static partial Task<string> WaitGetString();

    // Some return types require a [return: JSMarshalAs...] declaration indicating
    // the type mapping returned within the promise corresponding to Task<T>.
    [JSImport("PromisesShim.WaitGetDate", "PromisesShim")]
    [return: JSMarshalAs<JSType.Promise<JSType.Date>>()]
    public static partial Task<DateTime> WaitGetDate();

    [JSImport("PromisesShim.FetchCurrentUrl", "PromisesShim")]    
    public static partial Task<string> FetchCurrentUrl();

    [JSImport("PromisesShim.AsyncFunction", "PromisesShim")]
    public static partial Task AsyncFunction();

    [JSImport("PromisesShim.ConditionalSuccess", "PromisesShim")]
    public static partial Task ConditionalSuccess(bool shouldSucceed);

    // Supported
    [JSImport("PromisesShim.WaitGetArray", "PromisesShim")]
    [return: JSMarshalAs<JSType.Array<JSType.Number>>()]
    public static partial int[] GetIntArray();

    // Supported
    [JSImport("PromisesShim.WaitGetArray", "PromisesShim")]
    [return: JSMarshalAs<JSType.Promise<JSType.Number>>()]
    public static partial Task<int> WaitGetInt();



    // Not currently supported, too deeply nested generics in JSMarshalAs
    //[JSImport("PromisesShim.WaitGetArray", "PromisesShim")]
    //[return: JSMarshalAs<JSType.Promise<JSType.Array<JSType.Number>>>()]
    //public static partial Task<int[]> WaitGetIntArray();

    // Workaround, take the return from this call and pass it to UnwrapJSObjectAsIntArray
    // Return a JSObject reference to a JS number array
    [JSImport("PromisesShim.WaitGetIntArrayAsObject", "PromisesShim")]
    [return: JSMarshalAs<JSType.Promise<JSType.Object>>()]
    public static partial Task<JSObject> WaitGetIntArrayAsObject();

    // Takes a JSOBject reference to a JS number array, and returns the array as a C# int array.
    [JSImport("PromisesShim.UnwrapJSObjectAsIntArray", "PromisesShim")]
    [return: JSMarshalAs<JSType.Array<JSType.Number>>()]
    public static partial int[] UnwrapJSObjectAsIntArray(JSObject intArray);


}

public static class PromisesUsage
{
    public static async Task Run()
    {
        await JSHost.ImportAsync("PromisesShim", "https://localhost:7017/PromisesShim.js");
                
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
        catch(JSException ex) // Catch javascript exception
        {
            Console.WriteLine($"Javascript Exception Caught: '{ex.Message}'");
        }

        // Workaround for a Promise returning an array.
        JSObject arrayAsJSObject = await PromisesProxy.WaitGetIntArrayAsObject();          
        int[] intArray = PromisesProxy.UnwrapJSObjectAsIntArray(arrayAsJSObject);
        // Console WL the intArray
        Console.WriteLine($"intArray: {string.Join(", ", intArray)}");

    }
    // The example displays the following output in the browser's debug console:
    // Waited 2.0 seconds.
    // Waited .5 seconds for WaitGetString: 'String From Resolve'
    // Waited .5 seconds for WaitGetDate: '11/24/1988 12:00:00 AM'
    // responseText.Length: 582
    // Waited 2.0 seconds for AsyncFunction.
    // Javascript Exception Caught: 'Reject: ShouldSucceed == false'
    // intArray: 1, 2, 3, 4, 5

}