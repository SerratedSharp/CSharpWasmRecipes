using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;



namespace WasmBrowser.Recipes.WasmClient.Examples.RefactoredNames;

// Proxy that would hold any declarations if they existed at the root of the JS module.
// Also declares the module name.
internal partial class RefactoredNamesShimModule
{
    public const string ModuleName = "RefactoredNamesShim";
}

internal partial class PrimitivesProxy
{
    // This example reduces use of hardcoded strings in JSImport by using constants or `nameof()`.
    // The ModuleName is centralized and also used when calling ImportAsync.
    private const string moduleName = RefactoredNamesShimModule.ModuleName;
    private const string baseJSNamespace = $"{moduleName}.Primitives";
        
    [JSImport($"{baseJSNamespace}.{nameof(IncrementCounter)}", moduleName)]
    public static partial void IncrementCounter();

    [JSImport($"{baseJSNamespace}.{nameof(GetCounter)}", moduleName)]
    public static partial int GetCounter();

    [JSImport($"{baseJSNamespace}.LogValue", moduleName)]
    public static partial void LogInt(int value);

    [JSImport($"{baseJSNamespace}.LogValue", moduleName)]
    public static partial void LogString(string value);

    [JSImport($"{baseJSNamespace}.{nameof(LogValueAndType)}", moduleName)]
    public static partial void LogValueAndType([JSMarshalAs<JSType.Any>] object value);
}

public static class RefactoredNamesUsage
{
    public static async Task Run()
    {
        string baseUrl = JSHost.GlobalThis.GetPropertyAsJSObject("window")
            .GetPropertyAsJSObject("location").GetPropertyAsString("href");
        // Use module name constant, assume js name is same as module name.
        await JSHost.ImportAsync(RefactoredNamesShimModule.ModuleName,
            $"{baseUrl.TrimEnd('/')}/{RefactoredNamesShimModule.ModuleName}.js");

        PrimitivesProxy.IncrementCounter();      
        int counterValue = PrimitivesProxy.GetCounter();
        PrimitivesProxy.LogInt(counterValue);
        PrimitivesProxy.LogString("I'm a string from .NET in your browser!");
    }
}


internal static partial class GlobalProxy
{


    internal partial class HelpersProxy //: IHelpersProxy
    {
        private const string baseJSNamespace = "SerratedInteropHelpers.HelpersProxy";
        private const string moduleName = "SerratedInteropHelpers";


        [JSImport(baseJSNamespace + ".LoadjQuery", moduleName)]
        public static partial Task
            LoadJQuery(string relativeUrl);

        [JSImport(baseJSNamespace + ".LoadScript", moduleName)]
        public static partial Task
            LoadScript(string relativeUrl);

        //[JSImport(baseJSNamespace + ".LoadScriptWithContent", moduleName)]
        //public static partial Task
        //    LoadScriptWithContent(string scriptContent);


        // Used for unpacking an ArrayObject into a JSObject[] array
        [JSImport(baseJSNamespace + ".GetArrayObjectItems", moduleName)]
        [return: JSMarshalAs<JSType.Array<JSType.Object>>]
        public static partial JSObject[] GetArrayObjectItems(JSObject jqObject);
    }

    internal partial class HelpersProxyForUno //: IHelpersProxy
    {
        private const string baseJSNamespace = "globalThis." + "SerratedInteropHelpers.HelpersProxy";

        [JSImport(baseJSNamespace + ".LoadjQuery")]
        public static partial Task
            LoadJQuery(string relativeUrl);

        [JSImport(baseJSNamespace + ".LoadScript")]
        public static partial Task
            LoadScript(string relativeUrl);

        //[JSImport(baseJSNamespace + ".LoadScriptWithContent")]
        //public static partial Task
        //    LoadScriptWithContent(string scriptContent);

        // Used for unpacking an ArrayObject into a JSObject[] array
        [JSImport(baseJSNamespace + ".GetArrayObjectItems")]
        [return: JSMarshalAs<JSType.Array<JSType.Object>>]
        public static partial JSObject[] GetArrayObjectItems(JSObject jqObject);
    }
}

