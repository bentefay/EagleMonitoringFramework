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
            var element = createElement(item.value);

            $('#grid').append(element);

            $('#grid').gridList({
                lanes: 10,
                direction: "vertical",
                widthHeightRatio: 1,
                heightToFontSizeRatio: 0.25
            });
        });
    }
});

function createElement(text: string) {
    const item =
`<li data-w="1" data-h="1" data-x="0" data-y="0">
    <div class="inner">
        ${text}
    </div>
</li>`;

    return $(item);
}