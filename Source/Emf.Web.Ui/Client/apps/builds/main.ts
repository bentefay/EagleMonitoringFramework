/// <reference path="../../typings/all.d.ts"/>

import * as log from "../../common/log";
import { ObservableCollectionManager, IObservableRepositoryEvent } from "../../common/observable-collection-manager";
import $ = require("../../libs/jquery");
import "../../libs/jquery.gridlist";

log.logger.setLogLevel(log.LogLevel.Debug);
log.logger.logEvents.subscribe(new log.ConsoleObserver());

var manager = new ObservableCollectionManager("./signalr", { clearError: () => { }, showError: message => { } });

manager.subscribe("buildDefinitionReferences", {
    onNewEvent: event => {
        _.forEach(event.newOrUpdatedItems, item => {
            // document.write(JSON.stringify(item.value));
        });
    }
});

$(() => {

    $(".gridster ul").gridster({
        widget_margins: [10, 10],
        widget_base_dimensions: [140, 140]
    });

});