
import _ = require("lodash");
import moment = require("moment");

export class LogLevel {

    constructor(public id: string, private priority: number) {
    }

    isPriorityGreaterThanOrEqualTo(level: LogLevel) : boolean {
        return this.priority >= level.priority;
    }

    static Fatal = new LogLevel("Fatal", 5); 
    static Error = new LogLevel("Error", 4);
    static Warning = new LogLevel("Warning", 3);
    static Information = new LogLevel("Information", 2);
    static Debug = new LogLevel("Debug", 1);
    static Verbose = new LogLevel("Verbose", 0);
}

export interface TemplateNode { name: string, destructure: boolean, raw: string };
export interface TextNode { text: string };
export const isTemplateNode = (node: any): node is TemplateNode => { return typeof node.name === "string" };

export interface IMessagePropertyLookup {
    [propertyName: string]: any;
}

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

    bindProperties(positionalArgs: any[]): IMessagePropertyLookup {

        const result: IMessagePropertyLookup = {};

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

    render(properties: IMessagePropertyLookup, config: RenderConfig) {

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

    static create(level: LogLevel, messageTemplate: string, properties: any[], exception?: any): LogEvent {
        const parsedTemplate = new MessageTemplate(messageTemplate);
        const boundProperties = parsedTemplate.bindProperties(properties);
        return new LogEvent(moment(), level, parsedTemplate, boundProperties, exception);
    }

    constructor(public timestamp: moment.Moment, public level: LogLevel, public messageTemplate: MessageTemplate, public boundProperties: IMessagePropertyLookup,
        public exception?: any) {
    }

    clone(): LogEvent {
        return new LogEvent(this.timestamp, this.level, this.messageTemplate, <IMessagePropertyLookup>_.assign({}, this.boundProperties), this.exception);
    }

    renderedMessage(config: RenderConfig) {

        let renderedMessage = this.messageTemplate.render(this.boundProperties, config);

        if (this.exception)
            renderedMessage = renderedMessage.concat("\r\n", MessageTemplate.toText(this.exception, -1));

        return renderedMessage;
    }
}

type RenderConfig = { maxJsonLength: number };