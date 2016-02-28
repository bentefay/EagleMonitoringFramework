/// <reference path="../../typings/all.d.ts" />

import { LogEvent } from "./core";
import { IDisposable, Disposable, CompositeDisposable } from "../disposable";
import { IObservableFactory, Observable } from "../rx";

export class EnrichWithProperty implements IObservableFactory<LogEvent, LogEvent> {

    constructor(private _key: string, private _value: any) {}

    createObservable(observable): Observable<LogEvent> {
        return Observable.create<LogEvent>(observer => {
            return observable.subscribe(logEvent => {

                var clonedLogEvent = logEvent.clone();
                clonedLogEvent.boundProperties[this._key] = this._value;

                observer.onNext(clonedLogEvent);
            });
        });
    }
}