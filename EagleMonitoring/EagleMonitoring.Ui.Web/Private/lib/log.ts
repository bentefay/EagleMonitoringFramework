
import _ = require("lodash");

namespace Log {

    export class Logger {

        private listeners: { (event: LogEvent): void }[] = [];
        private logLevel = LogLevels.Info;

        setLogLevel(logLevel: Log.LogLevels) {
            this.logLevel = logLevel;
        }

        isLogLevelEnabled(logLevel: Log.LogLevels) {
            return logLevel <= this.logLevel;
        }

        event(logLevel: Log.LogLevels, format: string, args: string[], exception?: any) {

            if (!this.isLogLevelEnabled(logLevel))
                return;

            const newEvent = LogEvent.create(logLevel, format, args, exception);

            this.notifyAll(newEvent);
        }

        fatal(format: string, ...args: any[])                       { event(LogLevels.Fatal, format, args); }
        error(format: string, ...args: any[])                       { event(LogLevels.Error, format, args); }
        warn(format: string, ...args: any[])                        { event(LogLevels.Warn, format, args); }
        info(format: string, ...args: any[])                        { event(LogLevels.Info, format, args); }
        debug(format: string, ...args: any[])                       { event(LogLevels.Debug, format, args); }
        verbose(format: string, ...args: any[])                     { event(LogLevels.Verbose, format, args); }

        fatalEx(exception: any, format: string, ...args: any[])     { event(LogLevels.Fatal, format, args, exception); }
        errorEx(exception: any, format: string, ...args: any[])     { event(LogLevels.Error, format, args, exception); }
        warnEx(exception: any, format: string, ...args: any[])      { event(LogLevels.Warn, format, args, exception); }
        infoEx(exception: any, format: string, ...args: any[])      { event(LogLevels.Info, format, args, exception); }
        debugEx(exception: any, format: string, ...args: any[])     { event(LogLevels.Debug, format, args, exception); }
        verboseEx(exception: any, format: string, ...args: any[])   { event(LogLevels.Verbose, format, args, exception); }

        private notifyAll(event: LogEvent) {
            _.forEach(this.listeners, listener => {
                listener(event);
                return true;
            });
        }

        subscribe(handleLogEvent: (event: LogEvent) => void) {

            this.listeners.push(handleLogEvent);

            return { dispose: () => { this.listeners = _.without(this.listeners, handleLogEvent); } };
        }

        logTo(logEventListener: ILogEventListener) {
            return this.subscribe(event => logEventListener.handleLogEvent(event));
        }

        clearAllSubscriptions() {
            this.listeners = [];
        }
    }

    export interface TemplateNode { name: string, destructure: boolean, raw: string };
    export interface TextNode { text: string };
    export const isTemplateNode = (node: any): node is TemplateNode => { return typeof node.name === "string" };

    export class MessageTemplate {

        raw: string;
        tokens: (TextNode | TemplateNode)[];

        private static findProperties = /\{@?\w+}/g;

        constructor(messageTemplate: string) {

            this.raw = messageTemplate;
            // "Example {test} messageTemplate {@test2}"
            // tokens = [ { text: "Example " }, { name: "test", destructure: false, raw: "{test}" }, 
            //            { text: " messageTemplate " }, { name: "test2", destructure: true, raw: "{@test2}" } ]
            this.tokens = [];

            let result: RegExpExecArray;
            let textStart = 0;

            while ((result = MessageTemplate.findProperties.exec(messageTemplate)) !== null) {

                if (result && result.index !== textStart) {
                    this.tokens.push({ text: messageTemplate.slice(textStart, result.index) });
                }

                let destructure = false;
                let token = result[0].slice(1, -1);

                if (token.indexOf("@") === 0) {
                    token = token.slice(1);
                    destructure = true;
                }

                this.tokens.push({ name: token, destructure: destructure, raw: result[0] });
                textStart = MessageTemplate.findProperties.lastIndex;
            }

            if (textStart >= 0 && textStart < messageTemplate.length) {
                this.tokens.push({ text: messageTemplate.slice(textStart) });
            }

        }

        private capture(o, destructure?: boolean) {

            if (typeof o === "function")
                return o.toString();

            if (typeof o === "object") {
                // Could use instanceof Date, but this way will be kinder
                // to values passed from other contexts...
                if (destructure || typeof o.toISOString === "function") {
                    return o;
                }
                return o.toString();
            }
            return o;
        }

        bindProperties(positionalArgs: any[]) {

            const result = {};

            let nextArg = 0;
            for (let i = 0; i < this.tokens.length && nextArg < positionalArgs.length; ++i) {
                const token = this.tokens[i];
                if (isTemplateNode(token)) {
                    const p = positionalArgs[nextArg];
                    result[token.name] = this.capture(p, token.destructure);
                    nextArg++;
                }
            }

            while (nextArg < positionalArgs.length) {
                const px = positionalArgs[nextArg];
                if (typeof px !== "undefined") {
                    result["a" + nextArg] = this.capture(px);
                }
                nextArg++;
            }

            return result;
        }

        render(properties, config: RenderConfig) {

            const result = [];
            for (let i = 0; i < this.tokens.length; ++i) {
                const token = this.tokens[i];
                if (isTemplateNode(token)) {
                    if (properties.hasOwnProperty(token.name)) {
                        result.push(MessageTemplate.toText(properties[token.name], config.maxJsonLength));
                    } else {
                        result.push(token.raw);
                    }
                } else {
                    result.push(token.text);
                }
            }
            return result.join("");
        }

        static toText(o, maxJsonLength: number) {

            if (typeof o === "undefined")
                return "undefined";

            if (o === null)
                return "null";

            if (typeof o === "string")
                return o;

            if (typeof o === "number")
                return o.toString();

            if (typeof o === "boolean")
                return o.toString();

            if (typeof o.toISOString === "function")
                return o.toISOString();

            if (o instanceof Error) {
                const s = "".concat(o.stack);
                return s;
            }

            if (typeof o === "object") {
                let s = JSON.stringify(o);

                if (maxJsonLength > 0 && s.length > maxJsonLength)
                    s = s.slice(0, maxJsonLength - 3) + "...";

                return s;
            }

            return o.toString();
        }
    }





    export class LogEvent {

        timestamp: Date;
        level: LogLevels;
        messageTemplate: MessageTemplate;
        properties: {};
        exception: any;

        static create(level: LogLevels, messageTemplate: string, properties: any[], exception?: any): LogEvent {
            const parsedTemplate = new MessageTemplate(messageTemplate);
            const boundProperties = parsedTemplate.bindProperties(properties);
            return new LogEvent(new Date(), level, parsedTemplate, boundProperties, exception);
        }

        constructor(timestamp, level, messageTemplate, properties, exception?) {
            this.timestamp = timestamp;
            this.level = level;
            this.messageTemplate = messageTemplate;
            this.properties = properties;
            this.exception = exception;
        }

        renderedMessage(config: RenderConfig) {

            let renderedMessage = this.messageTemplate.render(this.properties, config);

            if (this.exception)
                renderedMessage = renderedMessage.concat("\r\n", MessageTemplate.toText(this.exception, -1));

            return renderedMessage;
        }
    }

    type RenderConfig = { maxJsonLength: number };

    export interface ILogEventListener {
        handleLogEvent(event: LogEvent);
    }

    export class ConsoleListener implements ILogEventListener {

        private maxJsonLength = -1;

        setMaxJsonLength(length: number) {
            this.maxJsonLength = length;
        }

        private log = this.createConsoleLogMethod("log");
        private info = this.createConsoleLogMethod("info");
        private warn = this.createConsoleLogMethod("warn");
        private error = this.createConsoleLogMethod("error");

        private createConsoleLogMethod(methodName): (args: any[]) => void {

            const console = window.console;

            if (!console)
                return _ => { };

            const method = console[methodName];

            if (!method) {
                if (methodName === "log")
                    return _ => { };
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

        handleLogEvent(event: LogEvent) {

            var args = [];
            args.push(event.renderedMessage({ maxJsonLength: this.maxJsonLength }));

            if (_.any(_.keys(event.properties))) {
                args.push("\r\n");
                args.push(event.properties);
            }

            switch (event.level) {
                case LogLevels.Fatal:
                case LogLevels.Error:
                    this.error(args);
                    break;
                case LogLevels.Warn:
                    this.warn(args);
                    break;
                case LogLevels.Info:
                    this.info(args);
                    break;
                case LogLevels.Debug:
                case LogLevels.Verbose:
                default:
                    this.log(args);
                    break;
            }
        }
    }

    export enum LogLevels { Fatal, Error, Warn, Info, Debug, Verbose }

    export var logger = new Logger();

    export var event = (errorLevel: Log.LogLevels, format: string, args: string[], exception?: any) =>
        logger.event(errorLevel, format, args, exception);

    export var fatal = (format: string, ...args: any[]) => event(LogLevels.Fatal, format, args);
    export var error = (format: string, ...args: any[]) => event(LogLevels.Error, format, args);
    export var warn = (format: string, ...args: any[]) => event(LogLevels.Warn, format, args);
    export var info = (format: string, ...args: any[]) => event(LogLevels.Info, format, args);
    export var debug = (format: string, ...args: any[]) => event(LogLevels.Debug, format, args);
    export var verbose = (format: string, ...args: any[]) => event(LogLevels.Verbose, format, args);

    export var fatalEx = (exception: any, format: string, ...args: any[]) => event(LogLevels.Fatal, format, args, exception);
    export var errorEx = (exception: any, format: string, ...args: any[]) => event(LogLevels.Error, format, args, exception);
    export var warnEx = (exception: any, format: string, ...args: any[]) => event(LogLevels.Warn, format, args, exception);
    export var infoEx = (exception: any, format: string, ...args: any[]) => event(LogLevels.Info, format, args, exception);
    export var debugEx = (exception: any, format: string, ...args: any[]) => event(LogLevels.Debug, format, args, exception);
    export var verboseEx = (exception: any, format: string, ...args: any[]) => event(LogLevels.Verbose, format, args, exception);

}

export = Log;