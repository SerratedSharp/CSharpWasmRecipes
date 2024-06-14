
// IIFE pattern is used to organize javascript code into namespaces.
// This is not the only way to declare JS for .NET import, but allows the namespace to conditionally be exported using
// AMD modules, ES6 modules, or traditional script referencing.  
// ES6 is the easiest module type to import into a project using the WasmBrowser template.

let PrimitivesShim = globalThis.PrimitivesShim || {};// Conditionally create namespace
(function (PrimitivesShim) {

    // Optionally create child namespaces
    //var Primitives = PrimitivesShim.Primitives || {};// create child namespace
    //Primitives.IncrementCounter = function () {
    //    counter += 1;
    //};
    //PrimitivesShim.Primitives = Primitives; // add to parent namespace

    globalThis.counter = 0;

    // Takes no parameters and returns nothing
    PrimitivesShim.IncrementCounter = function () {
        globalThis.counter += 1;
    };

    // Returns an int
    PrimitivesShim.GetCounter = () => globalThis.counter;
    // Identical with more verbose syntax:
    //Primitives.GetCounter = function () { return counter; };

    // Takes a parameter and returns nothing.  JS doesn't restrict the parameter type, but we can restrict it in the .NET proxy if desired.
    PrimitivesShim.LogValue = (value) => { console.log(value); };

    // Called for various .NET types to demonstrate mapping to JS primitive types
    PrimitivesShim.LogValueAndType = (value) => { console.log(typeof value, value); };
    
})(PrimitivesShim);

export { PrimitivesShim }; // Note: below second condition isn't working as expected, so always export'ing here

// Conditional export to support requireJS AMD modules, ES6 modules, or globally declared namespace for traditional script tag references
//if (typeof define === 'function' && define.amd) {
//    console.log("Defining ExamplesJSShim for AMD");
//    define(ExamplesJSShim);
//} else if ((typeof module === "object" && module.exports)) {
//    console.log("Exporting ExamplesJSShim for ES6");
//    module.exports = ExamplesJSShim;
//} else {
//    console.log("Declaring ExamplesJSShim in globalThis for <script>")
//    globalThis.ExamplesJSShim = ExamplesJSShim;
//}
