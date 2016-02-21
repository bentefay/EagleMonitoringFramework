
import { LogLevel, LogEvent } from "./logging/core";
import { Observable, IObserver, Subject } from "./logging/rx";
import { ConsoleObserver } from "./logging/console-observer"

export * from "./logging/core";
export * from "./logging/rx";
export * from "./logging/console-observer";

export class Logger implements IObserver<LogEvent> {

    private _logEvents = new Subject<LogEvent>();
    private _currentLogLevel = LogLevel.Information;

    logEvents: Observable<LogEvent>;

    constructor() {
        this.logEvents = this._logEvents;
    }

    setLogLevel(logLevel: LogLevel) {
        this._currentLogLevel = logLevel;
    }

    isLogLevelEnabled(logLevel: LogLevel) {
        return logLevel.isPriorityGreaterThanOrEqualTo(this._currentLogLevel);
    }

    onNext(event: LogEvent) {

        if (!this.isLogLevelEnabled(event.level))
            return;

        this._logEvents.onNext(event);
    }

    event(logLevel: LogLevel, format: string, args: string[], exception?: any) {

        if (!this.isLogLevelEnabled(logLevel))
            return;

        const newEvent = LogEvent.create(logLevel, format, args, exception);

        this._logEvents.onNext(newEvent);
    }

    fatal(format: string, ...args: any[])                       { this.event(LogLevel.Fatal, format, args); }
    error(format: string, ...args: any[])                       { this.event(LogLevel.Error, format, args); }
    warning(format: string, ...args: any[])                        { this.event(LogLevel.Warning, format, args); }
    information(format: string, ...args: any[])                        { this.event(LogLevel.Information, format, args); }
    debug(format: string, ...args: any[])                       { this.event(LogLevel.Debug, format, args); }
    verbose(format: string, ...args: any[])                     { this.event(LogLevel.Verbose, format, args); }

    fatalEx(exception: any, format: string, ...args: any[])     { this.event(LogLevel.Fatal, format, args, exception); }
    errorEx(exception: any, format: string, ...args: any[])     { this.event(LogLevel.Error, format, args, exception); }
    warningEx(exception: any, format: string, ...args: any[])      { this.event(LogLevel.Warning, format, args, exception); }
    informationEx(exception: any, format: string, ...args: any[])      { this.event(LogLevel.Information, format, args, exception); }
    debugEx(exception: any, format: string, ...args: any[])     { this.event(LogLevel.Debug, format, args, exception); }
    verboseEx(exception: any, format: string, ...args: any[])   { this.event(LogLevel.Verbose, format, args, exception); }
}

export var logger = new Logger();
export var internalLogger = new Logger();
internalLogger.logEvents.subscribe(new ConsoleObserver());

export var event = (errorLevel: LogLevel, format: string, args: string[], exception?: any) =>
    logger.event(errorLevel, format, args, exception);

export var fatal = (format: string, ...args: any[]) => event(LogLevel.Fatal, format, args);
export var error = (format: string, ...args: any[]) => event(LogLevel.Error, format, args);
export var warning = (format: string, ...args: any[]) => event(LogLevel.Warning, format, args);
export var information = (format: string, ...args: any[]) => event(LogLevel.Information, format, args);
export var debug = (format: string, ...args: any[]) => event(LogLevel.Debug, format, args);
export var verbose = (format: string, ...args: any[]) => event(LogLevel.Verbose, format, args);

export var fatalEx = (exception: any, format: string, ...args: any[]) => event(LogLevel.Fatal, format, args, exception);
export var errorEx = (exception: any, format: string, ...args: any[]) => event(LogLevel.Error, format, args, exception);
export var warningEx = (exception: any, format: string, ...args: any[]) => event(LogLevel.Warning, format, args, exception);
export var informationEx = (exception: any, format: string, ...args: any[]) => event(LogLevel.Information, format, args, exception);
export var debugEx = (exception: any, format: string, ...args: any[]) => event(LogLevel.Debug, format, args, exception);
export var verboseEx = (exception: any, format: string, ...args: any[]) => event(LogLevel.Verbose, format, args, exception);