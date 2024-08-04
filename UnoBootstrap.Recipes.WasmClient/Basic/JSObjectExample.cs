using System;
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
        public static partial string GetClass(JSObject elementObj);
                
        [JSImport("globalThis.JSON.parse")]
        public static partial JSObject GetJsonAsJSObject(string jsonString);



        [JSImport("globalThis.console.log")]
        public static partial void ConsoleLogJSObject(JSObject obj);

        [JSImport("globalThis.console.log")]
        public static partial void ConsoleLogUntyped([JSMarshalAs<JSType.Any>] object obj);

        [JSImport("globalThis.console.log")]
        public static partial void ConsoleLogUntypedArray([JSMarshalAs<JSType.Array<JSType.Any>>] object[] obj);

        public static void Log(params object[] parameters)
        {
            ConsoleLogUntypedArray(parameters);
        }

    }

    public partial class EventsInterop
    {

        [JSImport("globalThis.subscribeEvent")]
        public static partial JSObject SubscribeEvent(JSObject elementObj, string eventName,
            [JSMarshalAs<JSType.Function<JSType.Object>>] Action<JSObject> listener);

        [JSImport("globalThis.subscribeEventWithParameters")]
        public static partial void SusbcribeEventWithParameters(JSObject elementObj, string eventName,
            [JSMarshalAs<JSType.Function<JSType.Object, JSType.String, JSType.Object>>] Action<JSObject, string, JSObject> listener);

        [JSImport("globalThis.click")]
        public static partial void Click(JSObject elementObj);

        [JSImport("globalThis.unsubscribeEvent")]
        public static partial void UnsubscribeEvent(JSObject jSObject, string eventName, JSObject listener);
    }


}
