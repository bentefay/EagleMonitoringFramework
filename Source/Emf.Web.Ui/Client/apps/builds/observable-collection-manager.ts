import log = require("../../libs/log");
import _ = require("lodash");
import moment = require("moment");
import { DurationWithBackoff } from "../../libs/duration-with-backoff"

export class ObservableCollectionManagerArgs {

}

export class ObservableCollectionManager {

    private signalRStateLookup = {};
    private reconnectInterval = new DurationWithBackoff({ startingDuration: moment.duration(5, "seconds"), backoffFactor: 1.2, maxDuration: moment.duration(1, "minute") });

    constructor(signalRUrl: string) {

        const signalROptions = { transport: ["webSockets", "longPolling"], jsonp: false };

        const proxyHub = $.connection.liveGenerationDataHub;
        const hubs = $.connection.hub;
        hubs.url = signalRUrl;

        proxyHub.client.onNewDataPoints = () => { };

        // Uncomment for verbose SignalR logging
        // controlHub.logging = true;
        // More detailed errors can then be enabled on the server with:
        // var hubConfiguration = new HubConfiguration();
        // hubConfiguration.EnableDetailedErrors = true;
        // app.MapSignalR(hubConfiguration);

        this.signalRStateLookup = _.invert($.signalR.connectionState);

        hubs.connectionSlow(() => log.debug("SignalR connection slow"));

        hubs.error(error =>
            log.debug("SignalR error: {error}", error));

        hubs.stateChanged(change => {

            log.debug("SignalR connection change: {oldState} => {newState}", this.signalRStateLookup[change.oldState], this.signalRStateLookup[change.newState]);

            var connectionEnum = $.signalR.connectionState;

            switch (change.newState) {

                case connectionEnum.connecting:
                    break;

                case connectionEnum.reconnecting:
                    this.showError("Disconnected from server. Reconnecting...");
                    break;

                case connectionEnum.connected:
                    this.clearError();
                    this.reconnectInterval.reset();
                    break;

                case connectionEnum.disconnected:

                    const reconnectInterval = this.reconnectInterval.get();

                    this.showError(`Disconnected from server. Will attempt to reconnect in ${Math.round(reconnectInterval.asSeconds())} seconds...`, "error");

                    if (hubs.lastError) {
                        log.debug("Reason for disconnection: {message}", hubs.lastError.message);
                    }

                    log.debug("Will attempt to reconnect in {time} seconds", reconnectInterval.asSeconds());
                    setTimeout(() => {
                        hubs.start(signalROptions);
                    }, reconnectInterval.asMilliseconds());

                    this.reconnectInterval.increase();

                    break;
            }
        });

        proxyHub.client.initializeSubscriptions = () => {
            proxyHub.server.subscribe({})
                .fail((errorThrown) => {
                    this.showError(errorThrown);
                });
        }

        hubs.start(signalROptions);

    }
}