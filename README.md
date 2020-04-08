## <img src="icon.png" width="32"> &nbsp; PostSharp.Community.ToString 
**This add-in is not yet functioning!**

Auto-generates `ToString()` based on your class's fields and/or properties.  
#### Example
Your code:
```csharp
[ToString]
class TestClass
{
    public int Foo { get; set; }
    public double bar;
    
    [IgnoreDuringToString]
    public string Baz { get; set; }
}
```
What gets compiled:
```csharp
class TestClass
{
    public int Foo { get; set; }
    public double bar;    
    public string Baz { get; set; }
    
    public override string ToString()
    {
        return $"TestClass; Foo:{Foo},bar:{bar}";
    }
}
```

#### Installation
1. Install the NuGet package: `PM> Install-Package PostSharp.Community.ToString`
2. Get a free PostSharp Community license at https://www.postsharp.net/get/free
3. When you compile for the first time, you'll be asked to enter the license key.

#### How to use
Add `[ToString]` to the classes where you want to generate ToString, or use [multicasting](https://github.com/postsharp/Home/blob/master/multicasting.md).

You can also use `[IgnoreDuringToString]` to exclude some fields or properties from ToString.

You can also set some formatting options in the properties of the `ToString` attribute. You can set these formatting options
also globally by adding the assembly-wide attribute `[assembly:ToStringGlobalOptions]`.

#### Copyright notices
Published under the MIT license.

* Copyright © PostSharp Technologies, Simon Cropp, and contributors 
* Icon by <a href="https://www.flaticon.com/authors/smashicons" title="Smashicons">Smashicons</a> from <a href="https://www.flaticon.com/" title="Flaticon"> www.flaticon.com</a>