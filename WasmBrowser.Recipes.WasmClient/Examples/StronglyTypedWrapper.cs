using System;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;

namespace WasmBrowser.Recipes.WasmClient.Examples.StronglyTypedWrapper;

public partial class Document
{
    public static Element GetElementById(string id)
    {
        JSObject jsObject = DocumentProxy.GetElementById(id);
        return new Element(jsObject);// Wrap the JSObject in our Element wrapper.
    }
}

// Instance wrapper for JS Element
public partial class Element // : IJSObject
{
    internal JSObject jsObject;// reference to the javascript interop object

    /// <summary>
    /// Handle to the underlying javascript object.
    /// </summary>
    public JSObject JSObject => jsObject;

    // Internal/private constructor. Wrappers handle the two step process of retrieving and wrapping JSOBjects.
    private Element() { }

    // Copy constructor to allow wrapping of existing JSObject instances.  Consider making this internal unless you want to allow consumers to wrap arbitrary JSObjects directly.
    public Element(JSObject jsObject) 
    {
        // Optionally validate the JSObject is of the correct type if constructor is public.  Incurs an additional interop call.
        if (!ElementProxy.IsElement(jsObject))
            throw new ArgumentException("JSObject parameter does not refer to a JS Object of type Element.");

        this.jsObject = jsObject; 
    }

    public string GetAttribute(string attributeName)
        => ElementProxy.GetAttribute(jsObject, attributeName);

    public void SetAttribute(string attributeName, string value)
        => ElementProxy.SetAttribute(jsObject, attributeName, value);
    
    public void AppendHtml(string html) 
        => ElementProxy.AppendHtml(jsObject, html);
}

// Proxy that would hold any declarations if they existed at the root of the JS module.
// Also declares the module name.
internal partial class DomShimModule
{
    public const string ModuleName = "DomShim";
}

// Optionally make *Proxy classes internal in a class library so only the wrapper can see them.
// Or nest this as a Private class inside the wrapper class.
internal partial class ElementProxy
{
    // See wwwroot/DomShim.js for implementation details.
    private const string moduleName = DomShimModule.ModuleName;
    private const string baseJSNamespace = $"{moduleName}.Element";

    [JSImport($"{baseJSNamespace}.{nameof(GetAttribute)}", moduleName)]
    public static partial string GetAttribute(JSObject jSObject, string attributeName);

    [JSImport($"{baseJSNamespace}.{nameof(SetAttribute)}", moduleName)]
    public static partial void SetAttribute(JSObject jSObject, string attributeName, string value);

    [JSImport($"{baseJSNamespace}.{nameof(AppendHtml)}", moduleName)]
    public static partial void AppendHtml(JSObject jSObject, string html);
    
    [JSImport($"{baseJSNamespace}.{nameof(IsElement)}", moduleName)]
    public static partial bool IsElement(JSObject jSObject);
}

internal partial class DocumentProxy
{
    private const string moduleName = DomShimModule.ModuleName;
    private const string baseJSNamespace = $"{moduleName}.Document";

    [JSImport($"{baseJSNamespace}.{nameof(GetElementById)}", moduleName)]
    public static partial JSObject GetElementById(string id);
}

public static class StronglyTypedWrapperUsage
{
    public static async Task Run()
    {
        await JSHost.ImportAsync(DomShimModule.ModuleName, $"/{DomShimModule.ModuleName}.js");

        Element container = Document.GetElementById("testContainer");
        container.AppendHtml("<h1 id='header'>New Hello!</h1>");

        Document.GetElementById("header")
            .SetAttribute("style", "color: red;");
    }
}
// The example displays a red "New Hello!" element in the browser.



