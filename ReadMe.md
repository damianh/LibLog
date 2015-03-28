# LibLog [![Build status](https://ci.appveyor.com/api/projects/status/4v136j3od783udpa?svg=true)](https://ci.appveyor.com/project/damianh/liblog) [![NuGet Status](http://img.shields.io/nuget/v/LibLog.svg?style=flat)](https://www.nuget.org/packages/LibLog/)

[![Join the chat at https://gitter.im/damianh/LibLog](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/damianh/LibLog?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

A single file for you to either copy/paste or [install via nuget][0], into your library/framework/application to enable dependency free logging. It contains transparent built-in support for [NLog][3], [Log4Net][4], [EntLib Logging][5], [Serilog][9] and [Loupe][10] or allows the user to define a custom provider. 

When the DLL is added, It Just Works. i.e., all that's needed is to reference NLog / Log4Net in your app root so it ends up in the output directory and your component will automatically log without the user having to do any wireup or configuration. Loggers are either invoked dynamically (which is optimized) or via compiled expressions (also optimized).

The `ILog` interface consists of just 2 methods, which contrasts with the large interface (~65 members) in `Log4Net` and `Common.Logging`.

Nested Diagnostic Contexts(NDC) and Mapped Diagnostic Contexts (MDC) are available via `LogProvider.OpenNestedContext()` and `LogProvider.OpenMappedContext()`. Support has been implemented for NLog, Log4Net and Serilog. As fair as I know, EntLib Logging doesn't supported it (contact me if I'm wrong). Loupe support will come with next major version of Loupe.

### Why?

A logging _implementation_ is an application level concern. Your library / framework should not have any dependencies on any specific logging library, but may still need to support logging.

What about a shared `Common.Logging` you ask? No, you're still introducing a dependency that has the usual versioning concerns and adds yet another item to the consumers projects' references as well as a package reference.

### How?

The file provides:

* an `ILog` interface in your library that your code consumes.
* An `ILogProvider` your framework / library / application uses to get a logger.
* A static location where the application developer can set the `ILogProvider` and where you can retrieve it.

The included providers, `NLogProvider`, `Log4NetProvider`, `SerilogProvider`, `EntLibLogProvider`, `LoupeLogProvider` serve as templates for an application developer can follow to define their own custom provider.

### Using
* Copy [LibLog.cs][1] into your library and manually change the namespace OR [install the nuget package][0] which contains the source and which will automatically set the namespace to your project's root namespace.
* To get a current class logger:

```csharp
public class MyClass
{
    private static readonly ILog Logger = LogProvider.For<MyClass>(); 
    
    public MyClass()
    {
        using(LogProvider.OpenNestedContext("message"))
        using(LogProvider.OpenMappedContext("key", "value"))
        {
            Logger.Info(....);
        }
    }
}
```

Consumers can define their own provider in their application code to support custom loggers, decorate, etc:

```csharp
YourComponent.Logging.LogProvider.SetCurrentLogProvider(new CustomLogProvider())
```

### Example usages
 - [RavenDB][7]
 - [Thinktecture.IdentityServer.v3][8]

### License

LibLog is licensed under [MIT Licence][2].

##### Developed with:

[![Resharper](http://neventstore.org/images/logo_resharper_small.gif)](http://www.jetbrains.com/resharper/)
[![dotCover](http://neventstore.org/images/logo_dotcover_small.gif)](http://www.jetbrains.com/dotcover/)
[![dotTrace](http://neventstore.org/images/logo_dottrace_small.gif)](http://www.jetbrains.com/dottrace/)

Feedback, compliments or criticism: [@randompunter][6] 

[0]: https://www.nuget.org/packages/LibLog
[1]: https://github.com/damianh/LibLog/blob/master/src/LibLog/LibLog.cs
[2]: http://www.opensource.org/licenses/MIT
[3]: http://nlog-project.org/
[4]: https://logging.apache.org/log4net/
[5]: http://msdn.microsoft.com/en-us/library/ff647183.aspx
[6]: https://twitter.com/randompunter
[7]: https://github.com/ayende/ravendb/tree/master/Raven.Abstractions/Logging
[8]: https://github.com/IdentityServer/Thinktecture.IdentityServer3/tree/master/source/Core/Logging
[9]: http://serilog.net/
[10]: http://www.gibraltarsoftware.com/Loupe
