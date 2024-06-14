using System;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WasmBrowser.Recipes.WasmClient.Examples;

// See wwwroot/DateShim.js for implementation details.
public partial class DateProxy
{   
    [JSImport("DateShim.IncrementDay", "DateShim")]
    [return: JSMarshalAs<JSType.Date>] // Explicit JSMarshalAs for a return type.
    public static partial DateTime IncrementDay([JSMarshalAs<JSType.Date>] DateTime date);
    
    [JSImport("DateShim.LogValueAndType", "DateShim")]
    public static partial void LogValueAndType([JSMarshalAs<JSType.Date>] DateTime value);

    [JSImport("DateShim.FromDate", "DateShim")]
    [return: JSMarshalAs<JSType.Date>]
    public static partial DateTime FromDate([JSMarshalAs<JSType.Date>] DateTime value);

}

public static class DateUsage
{
    public static async Task Run()
    {
        // Ensure JS ES6 module loaded
        await JSHost.ImportAsync("DateShim", "/DateShim.js");

        // Basic examples of interop with a C# DateTime and JS Date.
        // Demonstrates affect of DateTimeKind on conversion.

        Console.WriteLine($"{NL}Increment Day");
        DateTime date = new DateTime(1968, 12, 21, 8, 51, 0, DateTimeKind.Utc);
        Console.WriteLine($".NET Date:  {date}, Kind: {date.Kind}, ToUTC: {date.ToUniversalTime()}");        
        DateProxy.LogValueAndType(date);
        DateTime fromJS = DateProxy.IncrementDay(date);        
        DateProxy.LogValueAndType(fromJS);

        Console.WriteLine($"{NL}Timezone Example (Kind.Local)");
        date = new DateTime(1968, 12, 21, 8, 51, 0, DateTimeKind.Local);
        Console.WriteLine($".NET Date: {date}, Kind: {date.Kind}, ToUTC: {date.ToUniversalTime()}");
        DateProxy.LogValueAndType(date);        
        fromJS = DateProxy.FromDate(date);
        Console.WriteLine($".NET Date: {fromJS}, Kind: {fromJS.Kind}, ToUTC: {fromJS.ToUniversalTime()}");
        DateProxy.LogValueAndType(fromJS);
        Console.WriteLine($"Are times the same? {TimesAreClose(date, fromJS)}");

        Console.WriteLine($"{NL}Timezone Example (Kind.Utc)");
        date = new DateTime(1968, 12, 21, 8, 51, 0, DateTimeKind.Utc);
        Console.WriteLine($".NET Date:  {date}, Kind: {date.Kind}, ToUTC: {date.ToUniversalTime()}");
        DateProxy.LogValueAndType(date);
        fromJS = DateProxy.FromDate(date);
        Console.WriteLine($".NET Date:  {fromJS}, Kind: {fromJS.Kind}, ToUTC: {fromJS.ToUniversalTime()}");
        DateProxy.LogValueAndType(fromJS);
        Console.WriteLine($"Are times the same? {TimesAreClose(date, fromJS)}");

        Console.WriteLine($"{NL}Timezone Example (Kind.Unspecified) assigned local time.");
        date = new DateTime(1968, 12, 21, 8, 51, 0);
        Console.WriteLine($".NET Date:  {date}, Kind: {date.Kind}, ToUTC: {date.ToUniversalTime()}");
        DateProxy.LogValueAndType(date);
        fromJS = DateProxy.FromDate(date);
        Console.WriteLine($".NET Date:  {fromJS}, Kind: {fromJS.Kind}, ToUTC: {fromJS.ToUniversalTime()}");
        DateProxy.LogValueAndType(fromJS);
        Console.WriteLine($"Are times the same? {TimesAreClose(date, fromJS)}");

        // Kind.Unspecified times are assumed to be local when converted to UTC, and assumed to be UTC when converted to local.
        // Assigning a time that represents a UTC without setting Kind.Utc, then passing it to JS will result in an incorrect time.
        Console.WriteLine($"{NL}Timezone Example (Kind.Unspecified) assigned UTC time.");
        date = new DateTime(1968, 12, 21, 12, 51, 0);        
        Console.WriteLine($".NET Date:  {date}, Kind: {date.Kind}, ToUTC: {date.ToUniversalTime()}");
        DateProxy.LogValueAndType(date);
        fromJS = DateProxy.FromDate(date);
        Console.WriteLine($".NET Date:  {fromJS}, Kind: {fromJS.Kind}, ToUTC: {fromJS.ToUniversalTime()}");
        DateProxy.LogValueAndType(fromJS);
        Console.WriteLine($"Are times the same? {TimesAreClose(date, fromJS)}");
    }

    public static bool TimesAreClose(DateTime date1, DateTime date2)
    {
        if ((date2.ToUniversalTime() - date1.ToUniversalTime()).TotalSeconds < 10.0)
            return true;
        else 
            return false;
    }

    static string NL = Environment.NewLine;
}
// The example displays the following output in the browser's debug console:
// Increment Day
// .NET Date:  12/21/1968 8:51:00 AM, Kind: Utc, ToUTC: 12/21/1968 8:51:00 AM
// Date: Sat Dec 21 1968 03:51:00 GMT-0500 (Eastern Standard Time)
// Date: Sun Dec 22 1968 03:51:00 GMT-0500 (Eastern Standard Time)
// 
// Timezone Example(Kind.Local)
// .NET Date: 12/21/1968 8:51:00 AM, Kind: Local, ToUTC: 12/21/1968 1:51:00 PM
// Date: Sat Dec 21 1968 08:51:00 GMT-0500 (Eastern Standard Time)
// .NET Date: 12/21/1968 1:51:00 PM, Kind: Utc, ToUTC: 12/21/1968 1:51:00 PM
// Date: Sat Dec 21 1968 08:51:00 GMT-0500 (Eastern Standard Time)
// Are times the same? True
// 
// Timezone Example(Kind.Utc)
// .NET Date:  12/21/1968 8:51:00 AM, Kind: Utc, ToUTC: 12/21/1968 8:51:00 AM
// Date: Sat Dec 21 1968 03:51:00 GMT-0500 (Eastern Standard Time)
// .NET Date:  12/21/1968 8:51:00 AM, Kind: Utc, ToUTC: 12/21/1968 8:51:00 AM
// Date: Sat Dec 21 1968 03:51:00 GMT-0500 (Eastern Standard Time)
// Are times the same? True
// 
// Timezone Example(Kind.Unspecified) assigned local time.
// .NET Date:  12/21/1968 8:51:00 AM, Kind: Unspecified, ToUTC: 12/21/1968 1:51:00 PM
// Date: Sat Dec 21 1968 08:51:00 GMT-0500 (Eastern Standard Time)
// .NET Date:  12/21/1968 1:51:00 PM, Kind: Utc, ToUTC: 12/21/1968 1:51:00 PM
// Date: Sat Dec 21 1968 08:51:00 GMT-0500 (Eastern Standard Time)
// Are times the same? True
// 
// Timezone Example(Kind.Unspecified) assigned UTC time.
// .NET Date:  12/21/1968 12:51:00 PM, Kind: Unspecified, ToUTC: 12/21/1968 5:51:00 PM
// Date: Sat Dec 21 1968 12:51:00 GMT-0500 (Eastern Standard Time)
// .NET Date:  12/21/1968 5:51:00 PM, Kind: Utc, ToUTC: 12/21/1968 5:51:00 PM
// Date: Sat Dec 21 1968 12:51:00 GMT-0500 (Eastern Standard Time)
// Are times the same? True
// 

