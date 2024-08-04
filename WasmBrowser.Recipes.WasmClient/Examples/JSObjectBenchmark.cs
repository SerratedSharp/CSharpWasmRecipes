using System;
using System.Diagnostics;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;

namespace WasmBrowser.Recipes.WasmClient.Examples;

public static class JSObjectBenchmark
{
    public static async Task Run()
    {
        //await JSHost.ImportAsync("JSObjectShim", "/JSObjectShim.js");

        //Stopwatch sw = new Stopwatch();

        //var jsObject = JSObjectProxy.CreateObject();
        //sw.Start();
        //for (int i = 0; i < 1000000; i++)
        //{
        //    JSObjectProxy.IncrementAnswer(jsObject);
        //}
        //sw.Stop();
        //Console.WriteLine($"JS interop elapsed time: {sw.Elapsed.TotalSeconds:#.0000} seconds at {sw.Elapsed.TotalMilliseconds / 1000000d:#.000000} ms per operation");

        //var pocoObject = new PocoObject { Question = "What is the answer?", Answer = 41 };
        //sw.Restart();
        //for (int i = 0; i < 1000000; i++)
        //{
        //    pocoObject.IncrementAnswer();
        //}
        //sw.Stop();
        //Console.WriteLine($".NET elapsed time: {sw.Elapsed.TotalSeconds:#.0000} seconds at {sw.Elapsed.TotalMilliseconds / 1000000d:#.000000} ms per operation");


        //Console.WriteLine($"Begin Object Creation");

        //sw.Restart();
        //for (int i = 0; i < 1000000; i++)
        //{
        //    var jsObject2 = JSObjectProxy.CreateObject();
        //    JSObjectProxy.IncrementAnswer(jsObject2);
        //}
        //sw.Stop();
        //Console.WriteLine($"JS interop elapsed time: {sw.Elapsed.TotalSeconds:#.0000} seconds at {sw.Elapsed.TotalMilliseconds / 1000000d:#.000000} ms per operation");

        //sw.Restart();
        //for (int i = 0; i < 1000000; i++)
        //{
        //    var pocoObject2 = new PocoObject { Question = "What is the answer?", Answer = 0 };
        //    pocoObject2.IncrementAnswer();
        //}
        //sw.Stop();
        //Console.WriteLine($".NET elapsed time: {sw.Elapsed.TotalSeconds:#.0000} seconds at {sw.Elapsed.TotalMilliseconds / 1000000d:#.000000} ms per operation");
    }
    
    public class PocoObject // Plain old CLR object
    {
        public string Question { get; set; }
        public int Answer { get; set; }

        public void IncrementAnswer() => Answer += 1;        
    }
}
// The example displays the following output in the browser's debug console:
// JS interop elapsed time: .2536 seconds at .000254 ms per operation
// .NET elapsed time: .0210 seconds at .000021 ms per operation
// Begin Object Creation
// JS interop elapsed time: 2.1686 seconds at .002169 ms per operation
// .NET elapsed time: .1089 seconds at .000109 ms per operation

