This directory contains examples of extending functionality of loggers. This is needed to pass 
extra per-call context information to the logging engine. For example, you can pass log event ID 
that will be written to the Event Log.

There are 2 methods of extending loggers:
* wrapping loggers - the code that wraps Logger is located in the LoggerWrapper directory.
* inheriting from loggers - the code that extends the Logger class by inheriting from it can be found in the InheritFromLogger directory.

Both methods construct a <code>LogEventInfo</code> object and pass it to the Log() method of the logger. It is important
to also pass the declaring type of the method that is invoked by user code, otherwise the ${callsite} 
and ${stacktrace} layout renderers won't work properly.

The examples also demonstrate is the technique of passing additional per-log context information. This is
done by adding items to the <code>LogEvent.Context</code> dictionary. This context data can be retrieved 
and used in layouts with the <code>${event-context}</code> layout renderer.
