
let ProxylessDomShim = globalThis.ProxylessDomShim || {};// Conditionally create namespace
(function (ProxylessDomShim) {

    let Element = ProxylessDomShim.Element || {};// create child namespace
    ProxylessDomShim.Element = Element; // add to parent namespace

    let Document = ProxylessDomShim.Document || {};// create child namespace
    ProxylessDomShim.Document = Document; // add to parent namespace

    //Element.GetAttribute = (elementObj, attributeName) =>
    //    elementObj.getAttribute(attributeName);

    //Element.SetAttribute = (elementObj, attributeName, attributeValue) =>
    //    elementObj.setAttribute(attributeName, attributeValue);

    Element.AppendHtml = (elementObj, htmlString) => {               
        elementObj.innerHTML += htmlString;
    }

    Document.GetElementById = (elementId) =>
        globalThis.document.getElementById(elementId);

})(ProxylessDomShim);

export { ProxylessDomShim }; 
