/// <reference path="tsd.d.ts" />

declare function require(id: string): any;

// SignalR extensions

interface SignalR {
    liveGenerationDataHub: any;
}

interface HubConnection {
    lastError: { name: string, source: string, message: string, stack: any };
}

// Kendo extensions

declare module kendo {
}

declare module "kendo" {
    export = kendo;
}