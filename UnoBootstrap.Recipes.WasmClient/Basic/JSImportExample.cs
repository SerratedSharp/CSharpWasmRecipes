using System.Runtime.InteropServices.JavaScript;

namespace UnoBootstrap.Recipes.WasmClient.Basic
{
    public partial class JSImportExample
    {
        [JSImport("globalThis.console.log")]
        public static partial void GlobalThisConsoleLog(string text);
    }
}
