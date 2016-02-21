
import { LogEvent } from "./core";
import { IObserver } from "./rx";

export class BufferingObserver implements IObserver<LogEvent> {

    private _events: LogEvent[] = [];

    getEvents() { return this._events; }

    onNext(event: LogEvent) { this._events.push(event); }

    writeEventsTo(observer: IObserver<LogEvent>) {
        for (const event of this._events) {
            observer.onNext(event);
        }
    }
}
