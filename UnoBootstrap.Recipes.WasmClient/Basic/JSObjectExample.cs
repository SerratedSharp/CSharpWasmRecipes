using System.Runtime.InteropServices.JavaScript;

namespace UnoBootstrap.Recipes.WasmClient.Basic
{

    //[SupportedOSPlatform("browser")]
    // WARNING: This Uno namespace contains a different JSObject declaration which is not the one we want here: using Uno.Foundation.Interop;
    // Ensure you include using System.Runtime.InteropServices.JavaScript; or fully qualify.
    public partial class JSObjectExample
    {
        [JSImport("globalThis.findElement")]
        public static partial JSObject FindElement(string id);

        [JSImport("globalThis.getClass")]
        public static partial string GetClass(JSObject obj);
    }

    
}
