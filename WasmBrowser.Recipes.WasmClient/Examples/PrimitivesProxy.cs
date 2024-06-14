using System;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;

namespace WasmBrowser.Recipes.WasmClient.Examples;

// See wwwroot/PrimitivesShim.js for implementation details.
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
        await JSHost.ImportAsync("PrimitivesShim", "https://localhost:7017/PrimitivesShim.js");
        
        // Call a proxy to a static JS method, console.log("")
        PrimitivesProxy.ConsoleLog("Printed from JSImport of console.log()");

        // Basic examples of JS interop with an integer:       
        PrimitivesProxy.IncrementCounter();
        int counterValue = PrimitivesProxy.GetCounter();
        PrimitivesProxy.LogInt(counterValue);
        PrimitivesProxy.LogString("I'm a string from .NET in your browser!");

        // Mapping some other .NET types to JS primitives:
        // See types table under https://learn.microsoft.com/en-us/aspnet/core/client-side/dotnet-interop?view=aspnetcore-8.0#javascript-interop-on-
        PrimitivesProxy.LogValueAndType(true);
        PrimitivesProxy.LogValueAndType(0x3A);// byte literal
        PrimitivesProxy.LogValueAndType('C');
        PrimitivesProxy.LogValueAndType((Int16)12);
        // Note: Javascript Number has a lower max value and can generate overflow errors
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


