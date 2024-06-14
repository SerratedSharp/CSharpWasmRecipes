using System.Runtime.InteropServices.JavaScript;

namespace RecipesLibrary;

// JSImport/Export classes and methods are marked `partial` since the interop implementation is generated dynamically
// Projects must be compiled with the /unsafe flag, or include <PropertyGroup><AllowUnsafeBlocks>true</AllowUnsafeBlocks></PropertGroup> in the project file.
public partial class GlobalThisProxyJS
{

    // Map to an existing global function
    [JSImport("window.location.href")]
    public static partial string GetHref();

}

//public partial class GlobalProxy

