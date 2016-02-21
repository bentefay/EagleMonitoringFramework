import log = require("./log");
import _ = require("lodash");
import moment = require("moment");
import { DurationWithBackoff } from "./duration-with-backoff"
import { IStatefulErrorHandler } from "./stateful-error-handler";
import { Disposable, IDisposable } from "./disposable";
import "../libs/signalr";
import $ = require("../libs/jquery");

export class ObservableCollectionManager {

    private signalRStateLookup = {};
    private reconnectInterval = new DurationWithBackoff({ startingDuration: moment.duration(5, "seconds"), backoffFactor: 1.2, maxDuration: moment.duration(1, "minute") });
    private repositoryIds: string[] = [];
    private subscriptionsByRepositoryId: { [repositoryId: string]: { observer: IEventObserver, subscriptionId: number } } = {};
    private hubs: HubConnection;
    private proxyHub;
    private subscriptionCount = 0;
    private initialized = false;

    constructor(signalRUrl: string, private statefulErrorHandler: IStatefulErrorHandler) {

        const signalROptions = { transport: ["webSockets", "longPolling"], jsonp: false };

        this.proxyHub = $.connection.repositories;
        this.hubs = $.connection.hub;
        this.hubs.url = signalRUrl;

        // Uncomment for verbose SignalR logging
        this.hubs.logging = true;
        // More detailed errors can then be enabled on the server with:
        // var hubConfiguration = new HubConfiguration();
        // hubConfiguration.EnableDetailedErrors = true;
        // app.MapSignalR(hubConfiguration);

        this.signalRStateLookup = _.invert($.signalR.connectionState);

        this.hubs.connectionSlow(() => log.debug("SignalR connection slow"));

        this.hubs.error(error =>
            log.debug("SignalR error: {error}", error));

        this.hubs.stateChanged(change => {

            log.debug("SignalR connection change: {oldState} => {newState}", this.signalRStateLookup[change.oldState], this.signalRStateLookup[change.newState]);

            var connectionEnum = $.signalR.connectionState;

            switch (change.newState) {

                case connectionEnum.connecting:
                    break;

                case connectionEnum.reconnecting:
                    statefulErrorHandler.showError("Disconnected from server. Reconnecting...");
                    break;

                case connectionEnum.connected:
                    statefulErrorHandler.clearError();
                    this.reconnectInterval.reset();
                    break;

                case connectionEnum.disconnected:

                    this.initialized = false;

                    const reconnectInterval = this.reconnectInterval.get();

                    statefulErrorHandler.showError(`Disconnected from server. Will attempt to reconnect in ${Math.round(reconnectInterval.asSeconds())} seconds...`);

                    if (this.hubs.lastError) {
                        log.debug("Reason for disconnection: {message}", this.hubs.lastError.message);
                    }

                    log.debug("Will attempt to reconnect in {time} seconds", reconnectInterval.asSeconds());
                    setTimeout(() => {
                        this.hubs.start(signalROptions);
                    }, reconnectInterval.asMilliseconds());

                    this.reconnectInterval.increase();

                    break;
            }
        });

        this.proxyHub.client.initializeSubscriptions = () => {
            _.forEach(this.repositoryIds, repositoryId => {
                this._subscribe(this.subscriptionsByRepositoryId[repositoryId].subscriptionId, repositoryId);
                return true;
            });

            this.initialized = true;
        };

        this.proxyHub.client.onNewEvent = (repositoryId: string, event: IObservableRepositoryEvent) => {
            var observer = this.subscriptionsByRepositoryId[repositoryId].observer;
            if (observer)
                observer.onNewEvent(event);
        };

        this.hubs.start(signalROptions);
    }

    subscribe(repositoryId: string, observer: IEventObserver): IDisposable {

        const subscriptionId = this.subscriptionCount++;

        if (this.subscriptionsByRepositoryId[repositoryId])
            throw new Error(`Already subscribed to repository with id ${repositoryId}`);
            
        this.repositoryIds.push(repositoryId);
        this.subscriptionsByRepositoryId[repositoryId] = { observer, subscriptionId };

        if (this.initialized) {
            this._subscribe(subscriptionId, repositoryId);
        }

        return new Disposable(() => {

            _.remove(this.repositoryIds, repositoryId);
            delete this.subscriptionsByRepositoryId[repositoryId];

            if (this.initialized) {
                this.proxyHub.server
                    .unsubscribe(subscriptionId)
                    .fail((errorThrown) => {
                        this.statefulErrorHandler.showError(errorThrown);
                    });
            }
        });
    }

    private _subscribe(subscriptionId: number, repositoryId: string) {
        return this.proxyHub.server
            .subscribe(subscriptionId, { repositoryId: repositoryId })
            .fail((errorThrown) => {
                this.statefulErrorHandler.showError(errorThrown);
            });
    }
}

export interface IEventObserver {
    onNewEvent(event: IObservableRepositoryEvent): void;
}

export interface IKeyValue {
    key: string;
    value: any;
}

export interface IObservableRepositoryEvent {
    newOrUpdatedItems: IKeyValue[];
    deletedItemKeys: string[];
    reset: boolean;
}

