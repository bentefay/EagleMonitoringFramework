
import _ = require("lodash");

import { LogEvent, LogLevel } from "./core";
import { IObserver } from "./rx";

export class ConsoleObserver implements IObserver<LogEvent> {

    private _maxJsonLength = -1;

    setMaxJsonLength(length: number) {
        this._maxJsonLength = length;
    }

    private _log = this.createConsoleLogMethod("log");
    private _info = this.createConsoleLogMethod("info");
    private _warn = this.createConsoleLogMethod("warn");
    private _error = this.createConsoleLogMethod("error");

    private createConsoleLogMethod(methodName): (args: any[]) => void {

        const console = window.console;

        if (!console)
            return () => { };

        const method = console[methodName];

        if (!method) {
            if (methodName === "log")
                return () => { };
            else
                return this.createConsoleLogMethod("log");
        }

        if (method.apply) {
            return args => {
                try {
                    method.apply(console, args);
                } catch (e) {
                }
            }
        } else {
            return args => {
                try {
                    Function.prototype.apply.apply(method, [console, args]);
                } catch (e) {
                }
            }
        }
    }

    onNext(event: LogEvent) {

        const args = [];
        args.push(event.renderedMessage({ maxJsonLength: this._maxJsonLength }));

        if (_.some(_.keys(event.boundProperties))) {
            args.push("\r\n");
            args.push(event.boundProperties);
        }

        if (event.exception)
            args.push(event.exception);

        switch (event.level) {
            case LogLevel.Fatal:
            case LogLevel.Error:
                this._error(args);
                break;
            case LogLevel.Warning:
                this._warn(args);
                break;
            case LogLevel.Information:
                this._info(args);
                break;
            case LogLevel.Debug:
            case LogLevel.Verbose:
            default:
                this._log(args);
                break;
        }
    }
}