using System;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using Uno.Foundation;
//using Uno.Foundation.Interop;
using System.Runtime.InteropServices.JavaScript;

namespace UnoBootstrap.Recipes.WasmClient.Basic
{

    public static class InvokeJSExamples
    {
        public static void ConsoleLog()
        {
            // InvokeJS calling console.log() with a single string parameter
            WebAssemblyRuntime.InvokeJS($"""console.log("Called from C#");""");
            // Triple """ allows us to use " in the string without escaping them
        }

        public static void ConsoleLogWithParam(string message)
        {
            // InvokeJS calling console.log() with a single string parameter
            WebAssemblyRuntime.InvokeJS($"""console.log("{message}");""");           

            // Note: The parameter in this case should be from a trusted source or there is
            // a potential for XSS.  Please see Security.cs for more details.
        }

        public static void ConsoleLogSafe(string message)
        {
            string sanitized = WebAssemblyRuntime.EscapeJs(message);
            WebAssemblyRuntime.InvokeJS($"""console.log("{sanitized}");""");
        }

    }


}
