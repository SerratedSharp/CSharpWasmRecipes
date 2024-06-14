using System;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
using SerratedSharp.JSInteropHelpers;

namespace WasmBrowser.Recipes.WasmClient.Examples.ProxylessWrapper;

public partial class Document
{
    public static Element GetElementById(string id)
    {
        JSObject jsObject = DocumentProxy.GetElementById(id);
        return new Element(jsObject);// Wrap the JSObject in our Element wrapper.
    }
}

// Instance wrapper for JS Element
public partial class Element : IJSObjectWrapper<Element>
{
    internal JSObject jsObject;// reference to the javascript interop object

    /// <summary>
    /// Handle to the underlying javascript object.
    /// </summary>
    public JSObject JSObject => jsObject;

    // Private constructor. Wrappers handle the two step process of retrieving and wrapping JSOBjects.
    private Element() { } 
    public Element(JSObject jsObject) { this.jsObject = jsObject; }

    // This static factory method defined by the IJSObjectWrapper enables generic code such as CallJSOfSameNameAsWrapped to automatically wrap JSObjects
    static Element IJSObjectWrapper<Element>.WrapInstance(JSObject jsObject) 
        => new Element(jsObject);
    
    public string GetAttribute(string attributeName)
       => this.CallJSOfSameName<string>(attributeName);

    public void SetAttribute(string attributeName, string value)
       => this.CallJSOfSameName<object>(attributeName, value);

    public void AppendHtml(string html)
       => ElementProxy.AppendHtml(this.JSObject, html);
}

// Proxy that would hold any declarations if they existed at the root of the JS module.
// Also declares the module name.
internal partial class ProxylessDomShimModule
{
    public const string ModuleName = "ProxylessDomShim";
}


internal partial class ElementProxy
{
    // See wwwroot/DomShim.js for implementation details.
    private const string moduleName = ProxylessDomShimModule.ModuleName;
    private const string baseJSNamespace = $"{moduleName}.Element";
    // No longer needed due to use of CallJSOfSameName from SerratedSharp.JSInteropHelpers
    //    [JSImport($"{baseJSNamespace}.{nameof(GetAttribute)}", moduleName)]
    //    public static partial string GetAttribute(JSObject jSObject, string attributeName);

    //    [JSImport($"{baseJSNamespace}.{nameof(SetAttribute)}", moduleName)]
    //    public static partial void SetAttribute(JSObject jSObject, string attributeName, string value);

    [JSImport($"{baseJSNamespace}.{nameof(AppendHtml)}", moduleName)]
    public static partial void AppendHtml(JSObject jSObject, string html);

}

internal partial class DocumentProxy
{
    private const string moduleName = ProxylessDomShimModule.ModuleName;
    private const string baseJSNamespace = $"{moduleName}.Document";

    [JSImport($"{baseJSNamespace}.{nameof(GetElementById)}", moduleName)]
    public static partial JSObject GetElementById(string id);
}

public static class ProxylessWrapperUsage
{
    public static async Task Run()
    {
        await JSHost.ImportAsync(ProxylessDomShimModule.ModuleName, $"/{ProxylessDomShimModule.ModuleName}.js");

        var container = Document.GetElementById("testContainer");
        container.AppendHtml("<h1 id='header2'>Proxyless Wrapper Hello!</h1>");
        Document.GetElementById("header2")
            .SetAttribute("style", "color: blue;");
    }
}
// The example displays a blue "Proxyless Wrapper Hello!" element in the browser.



