# LibLog [![Build status](https://ci.appveyor.com/api/projects/status/4v136j3od783udpa?svg=true)](https://ci.appveyor.com/project/damianh/liblog) [![NuGet Badge](https://buildstats.info/nuget/LibLog)](https://www.nuget.org/packages/LibLog/) [![Join the chat at https://gitter.im/damianh/LibLog](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/damianh/LibLog?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

Designed specifically for library developers, `LibLog` is a single file for you to either copy/paste or [install via nuget][0], into your library/framework/application to provide a logging abstraction. It also contains transparent built-in support for [NLog][3], [Log4Net][4], [EntLib Logging][5], [Serilog][9] and [Loupe][10], and allows your users to define a custom provider if necessary.

Please see [Wiki](https://github.com/damianh/LibLog/wiki) for more information.

## Availability for NetStandard / new `.csproj` format

LibLog uses `.pp` file to do a namespace transform (aka [source transform](https://docs.microsoft.com/en-us/nuget/create-packages/source-and-config-file-transformations) ) so it fits into your project's namespace. However it appears that source transform are [currently broken in RTM](https://github.com/NuGet/Home/issues/4803). For now, the workaround:

 1. Copy [`LibLog.cs`](https://github.com/damianh/LibLog/blob/master/src/LibLog/LibLog.cs) to your netstandard project.
 2. Manually rename the namespace `YourRootNamespace` to your project's root namespace.
 3. Enable `LIBLOG_PORTABLE` compiler directive.

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
[9]: http://serilog.net/
[10]: http://www.gibraltarsoftware.com/Loupe
