/// <reference path="../typings/all.d.ts" />

import _ = require("lodash");

import { IDisposable, Disposable, CompositeDisposable } from "./disposable";

export * from "./disposable";

export interface IObserver<T> {
    onNext(value: T);
}

export class DelegateObserver<T> implements IObserver<T> {
    constructor(private _onNext: { (value: T): void }) {
    }

    onNext(value: T) { this._onNext(value); }
}

export abstract class Observable<T> {

    subscribe(observer: (IObserver<T> | { (value: T): void })): IDisposable {
        if (_.isFunction(observer)) {
            const onNext = <{ (value: T): void }>observer;
            return this._subscribe(new DelegateObserver<T>(onNext));
        } else {
            return this._subscribe(<IObserver<T>>observer);
        }
    }

    protected abstract _subscribe(observer: IObserver<T>): IDisposable;

    static create<T>(factory: (observer: IObserver<T>) => IDisposable): Observable<T> {
        return new DelegateObservable(factory);
    }

    where(predicate: (value: T) => boolean): Observable<T> {
        return Observable.create(o => {
            return this.subscribe(v => {
                if (predicate(v))
                    o.onNext(v);
            });
        });
    }

    then<TOut>(factory: ObservableFactory<T, TOut> | IObservableFactory<T, TOut>): Observable<TOut> {
        if (_.isFunction(factory)) {
            const factoryFunction = <ObservableFactory<T, TOut>>factory;
            return factoryFunction(this);
        } else {
            const factoryObject = <IObservableFactory<T, TOut>>factory;
            return factoryObject.createObservable(this);
        }
    }

    bufferWithTime(timeSpanInMilliseconds: number) {
        return Observable.create(o => {

            let buffer: T[] = [];
            let currentTimer: number = null;
            const timerDisposable = new Disposable(() => { if (currentTimer) clearTimeout(currentTimer) });

            const subscription = this.subscribe(v => {
                buffer.push(v);

                if (currentTimer === null) {
                    currentTimer = window.setTimeout(() => {
                        o.onNext(buffer);
                        buffer = [];
                        currentTimer = null;
                    }, timeSpanInMilliseconds);
                }
            });

            return new CompositeDisposable().add(timerDisposable).add(subscription);
        });
    }
}

export class DelegateObservable<T> extends Observable<T> {
    constructor(private _factory: (observer: IObserver<T>) => IDisposable) {
        super();
    }

    protected _subscribe(observer: IObserver<T>): IDisposable {
        return this._factory(observer);
    }
}

export class Subject<T> extends Observable<T> implements IObserver<T> {

    private _listeners: IObserver<T>[] = [];

    onNext(value: T) {
        _.forEach(this._listeners, listener => {
            listener.onNext(value);
            return true;
        });
    }

    protected _subscribe(observer: IObserver<T>): IDisposable {
        this._listeners.push(observer);
        return { dispose: () => { this._listeners = _.without(this._listeners, observer); } };
    }
}

type ObservableFactory<TIn, TOut> = { (observable: Observable<TIn>): Observable<TOut> }

export interface IObservableFactory<TIn, TOut> {
    createObservable(observable: Observable<TIn>): Observable<TOut>;
}