using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uno.Foundation;

namespace UnoBootstrap.Recipes.WasmClient.Basic
{
    internal class Security
    {
        // - User generated strings should only be embedded as parameters.
        // - User generated strings passed as string parameters in JS must be escaped with EscapeJs.
        // - User generated strings must be fenced in unescaped double quotes
        //      (the order you apply the escaping and then wrap in quotes is important)        

        public static void SafeInvokeJSWithStringParam()
        {
            // Imagine a user set their profile description to something malicious like this            
            // and then another user is viewing the page that displays the description,
            // and we are dynamically building HTML DOM for the description.
            // This kind of exploit attempts to break out of the double quotes
            // and execute arbitrary JS in the context of the viewing victim user.
            string userDescription = """
                ");alert("Can I do really bad stuff?");alert("
            """;

            // Exploit prevented by use of EscapeJs
            string sanitized = WebAssemblyRuntime.EscapeJs(userDescription);
            // in combination with fencing the string parameter in quotes.
            WebAssemblyRuntime.InvokeJS($"""
              var d = document.getElementById("first");                
              d.textContent("{sanitized}");
            """);

            // The rendered JS, all quotes within the sanitized parameter are escaped
            // AND the parameter is contained in our own real quotes.
            //  var d = document.getElementById("first");                
            //  d.textContent("\");alert(\"Can I do really bad stuff?\");alert(\"");
        }

        public static void UnsafeInvokeJSWithSingleQuotes()
        {
            // Single Quotes are tempting to use because they don't conflict with C# string quotes,
            string userDescription = "');alert('Can I do really bad stuff?');alert('";

            // but they are not currently escaped by EscapeJs            
            string jsSanitized = WebAssemblyRuntime.EscapeJs(userDescription);
            WebAssemblyRuntime.InvokeJS($@"
              var d = document.getElementById('first');
              d.textContent('{jsSanitized}');
            ");

            // The rendered JS, shows userDescription is able to breakout out of the parameter quotes
            // and execute arbitrary JS in the context of the viewing victim user.
            //  var d = document.getElementById('first');
            //  d.textContent('');alert('Can I do really bad stuff?');alert('');

        }

        public static void SafeInvokeJSWithSingleQuotes()
        {
            string userDescription = "');alert('Can I do really bad stuff?');alert('";

            // With our own additional escaping of single quotes, they can be used safely.
            string jsSanitized = WebAssemblyRuntime.EscapeJs(userDescription).Replace("'", "\'");
            WebAssemblyRuntime.InvokeJS($@"
              var d = document.getElementById('first');
              d.textContent('{jsSanitized}');
            ");

            // The rendered JS, shows userDescription attempt to break out of quotes is defeated by escaping            
            //  var d = document.getElementById('first');
            //  d.textContent('\');alert(\'Can I do really bad stuff?\');alert(\'');
        }

        // Be aware that sanitization and XSS protection is specific to where the content is being embedded.
        // If for example we were setting innerHTML instead, then there'd be additional HTML encoding required.
        // More security concerns apply if the context was embedded in an HTML attribute.
        // This is true of any approach used to manipulate the HTML DOM and not specific to WASM's InvokeJS.
        // It's the developer's responsibility to understand the security concerns of working with JS and HTML
        // appropriate to how they are manipulating the DOM.
        public static void SafeInvokeJSWithHtml()
        {
            string userDescription = "');alert('Can I do really bad stuff?');alert('";
            // Note the encodings are applied in the inverse order of the contexts they pass through.
            // This follows a last-in first-out pattern as if we are stacking encodings to be
            // popped in each rendering context.  The last encoding is appropriate for the first
            // context it is embedded in.
            // So it is HTML encoded, then JS encoded, then embedded in javascript, and then rendered as HTML.
            string htmlEncoded = System.Net.WebUtility.HtmlEncode(userDescription);
            string jsSanitized = WebAssemblyRuntime.EscapeJs(htmlEncoded);

            WebAssemblyRuntime.InvokeJS($"""
              var d = document.getElementById("first");
              d.innerHTML("{jsSanitized}");
            """);

        }


    }
}
