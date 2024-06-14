
// IIFE pattern is used to organize javascript code into namespaces.
// This is not the only way to declare JS for .NET import, but allows the namespace to conditionally be exported using
// AMD modules, ES6 modules, or traditional script referencing.  
// ES6 is the easiest module type to import into a project using the WasmBrowser template.

let JSObjectShim = globalThis.JSObjectShim || {};// Conditionally create namespace
(function (JSObjectShim) {

    JSObjectShim.CreateObject = function () {
        return {
            name: "Example JS Object",
            answer: 41,
            question: null,
            summarize: function () {
                return `The question is "${this.question}" and the answer is ${this.answer}.`;
            }
        };
    };
    
    JSObjectShim.IncrementAnswer = function (object) {
        object.answer += 1;
        // We don't return the modified object, since reference is modified.
    };

    // Proxy an instance method call.
    JSObjectShim.Summarize = function (object) {
        return object.summarize();        
    };

})(JSObjectShim);

export { JSObjectShim }; // Note: below second condition isn't working as expected, so always export'ing here.
// ES6 export has to be at the root.  Perhaps we can always do a ES6 export, then in additiona conditionally do one of the other exports.

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
