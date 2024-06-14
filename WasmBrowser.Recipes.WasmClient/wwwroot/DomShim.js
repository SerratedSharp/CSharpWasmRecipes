let DomShim = globalThis.DomShim || {};// Conditionally create namespace
(function (DomShim) {

    let ElementNS = DomShim.Element || {};// create child namespace
    DomShim.Element = ElementNS; // add to parent namespace

    let Document = DomShim.Document || {};// create child namespace
    DomShim.Document = Document; // add to parent namespace

    ElementNS.GetAttribute = (elementObj, attributeName) =>
        elementObj.getAttribute(attributeName);

    ElementNS.SetAttribute = (elementObj, attributeName, attributeValue) =>
        elementObj.setAttribute(attributeName, attributeValue);

    ElementNS.AppendHtml = (elementObj, htmlString) =>
        elementObj.innerHTML += htmlString;

    Document.GetElementById = (elementId) =>
        globalThis.document.getElementById(elementId);

    //Verify that obj is an instance of native Element
    ElementNS.IsElement = (obj) =>
        obj instanceof Element;


})(DomShim);

export { DomShim }; 