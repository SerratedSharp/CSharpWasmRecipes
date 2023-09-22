using System;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using Uno.Foundation;
using Uno.Foundation.Interop;



namespace UnoBootstrap.Recipes.WasmClient
{

    // TODO: Explain return types, return's "OK" unless you have a return sooner

    /// <summary>
    /// Used to make wrapping many JS functions easier, and to automatically convert and fence parameters.
    /// </summary>
    public static class InvokeJSHelpers
    {
        #region Examples. Also see InvokeJSHelpersExamples.cs for additional examples.

        // By using [CallerMemberName] we can easily create proxy calls for multiple functions by naming the caller the same as the JS function               
        public static void CreateElement(string elementName) => CallJSOfSameName("document", elementName);  // C#: Document.CreateElement("div"); JS: document.createElement('div');
        public static void GetElementById(string id) => CallJSOfSameName("document", id);  // C#: Document.GetElementById("someId"); JS: document.getElementById('someId');`
                
        // C#: Log("Some Values: ", 1, DateTime.Now)l; JS: console.log('Some Values: ', '1', '30/12/2023 07:53:53');        
        public static void Log(string message, object value, DateTime now)
           => InvokeJSHelpers.CallJSOfSameName("console", message, value, now);

        // ToParams is shorthand for new object[]{}, necesary we have more than 4 params.  Since CallJSOfSameName leveraged optional params, it cannot also use the params keyword.
        public static void Log(string messageBefore, object valueBefore, DateTime timeBefore, string messageAfter, object valueAfter, DateTime timeAfter)
           => InvokeJSHelpers.CallJSOfSameName("console", ToParams(messageBefore, valueBefore, timeBefore, messageAfter, valueAfter, timeAfter));

        // String are escaped/quoted, bools are serialized into JS lowercase, all other values are ToString'd then Escaped/Quoted, and finally concatenated into a JS parameter list.
        // See CallJSFunc and FormatParam for implementation details.

        #endregion

        public static string CallJSOfSameName(string objectName, object[] parameters, Breaker _ = default(Breaker), [CallerMemberName] string funcName = null)
        {
            // convert C# PascalCase func name to JS lowerCamelCase
            string lowerCamelCaseFuncName = Char.ToLowerInvariant(funcName[0]) + funcName.Substring(1);
            string lowerCamelCaseObjectName = Char.ToLowerInvariant(objectName[0]) + objectName.Substring(1);

            return CallJSFunc(lowerCamelCaseObjectName, lowerCamelCaseFuncName, parameters);
        }

        public static string CallJSFunc(string objectName, string funcName, params object[] parameters)
        {
            // Escape parameters and quote them if appropriate, and create comma seperated list            
            string jsParameters = string.Join(",", parameters.Select(p => $"{FormatParam(p)}"));
            string result = WebAssemblyRuntime.InvokeJSWithInterop(
                    $@"return {objectName}.{funcName}({jsParameters});"  // TODO Aaron: What does return do here for objects
            );

            return result;
        }

        public struct Breaker{ } // Prevents incorrect overload being used. See https://stackoverflow.com/a/26784846/84206

        // funcName gets populated with the name of the calling function
        public static string CallJSOfSameName(string objectName, Breaker _ = default(Breaker), [CallerMemberName] string funcName = null)
         => CallJSOfSameName(objectName, ToParams<object>(), default(Breaker), funcName);

        public static string CallJSOfSameName(string objectName, object parameter, Breaker _ = default(Breaker), [CallerMemberName] string funcName = null)
         => CallJSOfSameName(objectName, ToParams(parameter), default(Breaker), funcName);

        public static string CallJSOfSameName(string objectName, object param1, object param2, Breaker _ = default(Breaker), [CallerMemberName] string funcName = null)
         => CallJSOfSameName(objectName, ToParams(param1, param2), default(Breaker), funcName);

        public static string CallJSOfSameName(string objectName, object param1, object param2, object param3, Breaker _ = default(Breaker), [CallerMemberName] string funcName = null)
         => CallJSOfSameName(objectName, ToParams(param1, param2, param3), default(Breaker), funcName);

        public static string CallJSOfSameName(string objectName, object param1, object param2, object param3, object param4, Breaker _ = default(Breaker), [CallerMemberName] string funcName = null)
         => CallJSOfSameName(objectName, ToParams(param1, param2, param3, param4), default(Breaker), funcName);

        // Shorthand for new object[]{,,,}
        public static T[] ToParams<T>(params T[] args)
        {
            return args;
        }

        private static object FormatParam(object param)
        {
            string str = null;

            if (param is string s)
                str = "'" + WebAssemblyRuntime.EscapeJs(s).Replace("'", @"\'") + "'";// string params should be JS escaped and fenced in quotes
            else if (param is bool b)
                str = (b ? "true" : "false");// convert bools to lowercase JS true/false
            else if (param == null)
                str = "null";
            else if (param is IJSObject) // IJSObject's are left as-is for use in InveokJSWithInterop
                return param;
            else
                return FormatParam(param.ToString());// any other objects get ToString'd and passed back for string encoding/quoting

            return str;
        }


    }
}
