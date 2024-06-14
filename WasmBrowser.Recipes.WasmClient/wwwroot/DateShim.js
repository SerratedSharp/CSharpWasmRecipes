
// IIFE pattern is used to organize javascript code into namespaces.
// This is not the only way to declare JS for .NET import, but allows the namespace to conditionally be exported using
// AMD modules, ES6 modules, or traditional script referencing.  
// ES6 is the easiest module type to import into a project using the WasmBrowser template.

let DateShim = globalThis.DateShim || {};// Conditionally create namespace
(function (DateShim) {
    
    DateShim.IncrementDay = function (date) {
        date.setDate(date.getDate() + 1);
        return date;
    };
    
    DateShim.LogValueAndType = (value) => {
        if (value instanceof Date) 
            console.log("Date:", value)
        else
            console.log("Not a Date:", value)
    };

    DateShim.FromDate = (date) => date;
        
})(DateShim);

export { DateShim }; // Note: below second condition isn't working as expected, so always export'ing here

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
