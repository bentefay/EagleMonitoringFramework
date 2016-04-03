import log = require("./log");
import _ = require("lodash");
import moment = require("moment");
import { DurationWithBackoff } from "./duration-with-backoff"
import { IStatefulErrorHandler } from "./stateful-error-handler";
import { Disposable, IDisposable } from "./disposable";
import "../libs/signalr";
import $ = require("../libs/jquery");

export class ObservableCollectionManager {

    private _signalRStateLookup = {};
    private _reconnectInterval = new DurationWithBackoff({ startingDuration: moment.duration(5, "seconds"), backoffFactor: 1.2, maxDuration: moment.duration(1, "minute") });
    private _repositoryIds: string[] = [];
    private _subscriptionsByRepositoryId: { [repositoryId: string]: { observer: IEventObserver<any>, subscriptionId: number } } = {};
    private _hubs: HubConnection;
    private _proxyHub;
    private _subscriptionCount = 0;
    private _initialized = false;

    constructor(signalRUrl: string, private _statefulErrorHandler: IStatefulErrorHandler) {

        const signalROptions = { transport: ["webSockets", "longPolling"], jsonp: false };

        this._proxyHub = $.connection.repositories;
        this._hubs = $.connection.hub;
        this._hubs.url = signalRUrl;

        // Uncomment for verbose SignalR logging
        this._hubs.logging = false;
        // More detailed errors can then be enabled on the server with:
        // var hubConfiguration = new HubConfiguration();
        // hubConfiguration.EnableDetailedErrors = true;
        // app.MapSignalR(hubConfiguration);

        this._signalRStateLookup = _.invert($.signalR.connectionState);

        this._hubs.connectionSlow(() => log.debug("SignalR connection slow"));

        this._hubs.error(error =>
            log.debug("SignalR error: {error}", error));

        this._hubs.stateChanged(change => {

            log.debug("SignalR connection change: {oldState} => {newState}", this._signalRStateLookup[change.oldState], this._signalRStateLookup[change.newState]);

            var connectionEnum = $.signalR.connectionState;

            switch (change.newState) {

                case connectionEnum.connecting:
                    break;

                case connectionEnum.reconnecting:
                    _statefulErrorHandler.showError("Disconnected from server. Reconnecting...");
                    break;

                case connectionEnum.connected:
                    _statefulErrorHandler.clearError();
                    this._reconnectInterval.reset();
                    break;

                case connectionEnum.disconnected:

                    this._initialized = false;

                    const reconnectInterval = this._reconnectInterval.get();

                    _statefulErrorHandler.showError(`Disconnected from server. Will attempt to reconnect in ${Math.round(reconnectInterval.asSeconds())} seconds...`);

                    if (this._hubs.lastError) {
                        log.debug("Reason for disconnection: {message}", this._hubs.lastError.message);
                    }

                    log.debug("Will attempt to reconnect in {time} seconds", reconnectInterval.asSeconds());
                    setTimeout(() => {
                        this._hubs.start(signalROptions);
                    }, reconnectInterval.asMilliseconds());

                    this._reconnectInterval.increase();

                    break;
            }
        });

        this._proxyHub.client.initializeSubscriptions = () => {
            _.forEach(this._repositoryIds, repositoryId => {
                this._subscribe(this._subscriptionsByRepositoryId[repositoryId].subscriptionId, repositoryId);
                return true;
            });

            this._initialized = true;
        };

        this._proxyHub.client.onNewEvent = (repositoryId: string, event: IObservableRepositoryEvent<any>) => {
            var observer = this._subscriptionsByRepositoryId[repositoryId].observer;
            if (observer)
                observer.onNewEvent(event);
        };

        this._hubs.start(signalROptions);
    }

    subscribe<T>(repositoryId: string, observer: IEventObserver<T>): IDisposable {

        const subscriptionId = this._subscriptionCount++;

        if (this._subscriptionsByRepositoryId[repositoryId])
            throw new Error(`Already subscribed to repository with id ${repositoryId}`);
            
        this._repositoryIds.push(repositoryId);
        this._subscriptionsByRepositoryId[repositoryId] = { observer, subscriptionId };

        if (this._initialized) {
            this._subscribe(subscriptionId, repositoryId);
        }

        return new Disposable(() => {

            _.remove(this._repositoryIds, repositoryId);
            delete this._subscriptionsByRepositoryId[repositoryId];

            if (this._initialized) {
                this._proxyHub.server
                    .unsubscribe(subscriptionId)
                    .fail((errorThrown) => {
                        this._statefulErrorHandler.showError(errorThrown);
                    });
            }
        });
    }

    private _subscribe(subscriptionId: number, repositoryId: string) {
        return this._proxyHub.server
            .subscribe(subscriptionId, { repositoryId: repositoryId })
            .fail((errorThrown) => {
                this._statefulErrorHandler.showError(errorThrown);
            });
    }
}

export interface IEventObserver<T> {
    onNewEvent(event: IObservableRepositoryEvent<T>): void;
}

export interface IKeyValue<T> {
    key: string;
    value: T;
}

export interface IObservableRepositoryEvent<T> {
    newOrUpdatedItems: IKeyValue<T>[];
    deletedItemKeys: string[];
    reset: boolean;
}

