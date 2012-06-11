What?
====

A set of logging interfaces for you to copy and paste into your library or framework that allows dependency free logging with support for NLog, Log4Net or a custom provider. NLog / Log4Net are dynamically loaded if they are deployed. 

The ILog interface consists of just 2 methods, which contrasts with the large interface in Log4Net and Common.Logging.

Why?
====

From an operation perspective, the logging implementation is an application level concern. Your library / framework should not have any dependencies on any specific logging library. 

What about a shared Common.Logging you ask? No, you are still introducing a dependency that has the usual versioning concerns and adds yet another item to the consumers project's references as well as a package reference.

How?
====

* Provide an `ILog` interface in your library that your code consumes.
* An `ILogProvider` your framework / application used to get a logger.
* A static location where the application developer can set the `ILogProvider` and where you can retrieve it.
* Documentation or sample providers the application developer can add to their code base. See `NLogProvider` and `Log4NetProvider` for examples.

License
====

Logging is licensed under [MIT Licence][1].

[1]: http://www.opensource.org/licenses/MIT