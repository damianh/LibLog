# LibLog [![Build status](https://ci.appveyor.com/api/projects/status/4v136j3od783udpa?svg=true)](https://ci.appveyor.com/project/damianh/liblog) [![NuGet Badge](https://buildstats.info/nuget/LibLog)](https://www.nuget.org/packages/LibLog/) [![Join the chat at https://gitter.im/damianh/LibLog](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/damianh/LibLog?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

Designed specifically for library developers, `LibLog` is a single file for you to either copy/paste or [install via nuget][0], into your library/framework/application to provide a logging abstraction. It also contains transparent built-in support for [NLog][3], [Log4Net][4], [EntLib Logging][5], [Serilog][9] and [Loupe][10], and allows your users to define a custom provider if necessary.

Please see [Wiki](https://github.com/damianh/LibLog/wiki) for more information.

## Installation

### .NET Classic / Desktop (4.0 to 4.6.2)

[Install via nuget][0]

### .NET Standard / Core (NetStandard 1.1 and later)

Due to changes in how nuget packages are installed in nuget 4.0 / VS2017 / Core
SDK, namespace transforms are unfortunately no longer supported. This means the
LibLog nuget package will not work in .NET Standard / Core libraries as previously.
LibLog still works of course; however you need to install the file your self.

1. [Copy this installation script](download_liblog.ps1) into your project.
2. Tweak the `$rootNamespace` variable to your desired namespace.
3. Peridically run to download latest version of LibLog to your project.

## License

LibLog is licensed under [MIT Licence][2].

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
