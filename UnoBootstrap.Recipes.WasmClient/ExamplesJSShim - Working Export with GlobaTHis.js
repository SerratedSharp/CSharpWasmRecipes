
// IIFE pattern is used to organize javascript code into namespaces.
// This is not the only way to declare JS for .NET import, but allows the namespace to conditionally be exported using
// AMD modules, ES6 modules, or traditional script referencing.  
// ES6 is the easiest module type to import into a project using the WasmBrowser template.

let ExamplesJSShim = globalThis.ExamplesJSShim || {};// Conditionally create namespace
(function (ExamplesJSShim) {

    var Primitives = ExamplesJSShim.Primitives || {};// create child namespace

    let counter = 0;

    // Takes no parameters and returns nothing
    Primitives.IncrementCounter = function () {
        counter += 1;
    };

    // Returns an int
    Primitives.GetCounter = () => counter;
    // Identical to more verbose syntax:
    //Primitives.GetCounter = function () { return counter; };

    // Takes a parameter and returns nothing.  JS doesn't restrict the parameter type, but we can restrict it in the .NET proxy if desired.
    Primitives.LogValue = (value) => { console.log(value); };

    // Called for various .NEt type to demonstrate mapping to JS types
    Primitives.LogValueAndType = (value) => { console.log(typeof value, value); };

    ExamplesJSShim.Primitives = Primitives; // add to parent namespace

})(ExamplesJSShim = globalThis.ExamplesJSShim || (globalThis.ExamplesJSShim = {}));

export { ExamplesJSShim }; // Note: below second condition isn't working as expected, so always export'ing here


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
