# LibLog [![NuGet Status](http://img.shields.io/nuget/v/LibLog.svg?style=flat)](https://www.nuget.org/packages/LibLog/)

A single file for you to either copy and paste, or [install via nuget][0], into your library, framework or application that allows dependency free logging with support for NLog, Log4Net or allow the user to define a custom provider. NLog / Log4Net are dynamically loaded if they are deployed. 

The ILog interface consists of just 2 methods, which contrasts with the large interface (~65 members) in Log4Net and Common.Logging.

### Why?

A logging _implementation_ is an application level concern. Your library / framework should not have any dependencies on any specific logging library, but may still need to support logging.

What about a shared Common.Logging you ask? No, you are still introducing a dependency that has the usual versioning concens and adds yet another item to the consumers projects' references as well as a package reference.

### How?

* Provide an `ILog` interface in your library that your code consumes.
* An `ILogProvider` your framework / library / application uses to get a logger.
* A static location where the application developer can set the `ILogProvider` and where you can retrieve it.
* See the included providers, `NLogProvider` and `Log4NetProvider`, as examples an application developer can follow to define their own custom provider.

### Using
* Copy [Logging.cs][1] into your library and manually change the namespace, or, [install the nuget package][0] which contains the source and which will automatically set the namespace to your project's root namespace.
* To get a current class logger:

```csharp
public class MyClass
{
    private static readony ILog Logger = LogProvider.GetCurrentClassLogger();
    
    public MyClass()
    {
        Logger.Info(....);
    }
}
```

### License

Logging is licensed under [MIT Licence][2].

[0]: https://www.nuget.org/packages/LibLog
[1]: https://github.com/damianh/LibLog/blob/master/src/LibLog/Logging.cs
[2]: http://www.opensource.org/licenses/MIT
