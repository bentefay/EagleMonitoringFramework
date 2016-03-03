/// <reference path="../../typings/all.d.ts"/>

import * as log from "../../common/log";
import { ObservableCollectionManager, IObservableRepositoryEvent } from "../../common/observable-collection-manager";
import $ = require("../../libs/jquery");
import React = require("react");
import ReactDOM = require("react-dom");

log.logger.setLogLevel(log.LogLevel.Debug);
log.logger.logEvents.subscribe(new log.ConsoleObserver());

var manager = new ObservableCollectionManager("./signalr", { clearError: () => { }, showError: message => { } });

var list: { [key: string]: IBuildDefinitionReferenceDto } = {};

manager.subscribe<IBuildDefinitionReferenceDto>("buildDefinitionReferences", {
    onNewEvent: event => {
        _.forEach(event.newOrUpdatedItems, item => {
            list[item.key] = item.value;
        });

        _.forEach(event.deletedItemKeys, key => {
            delete list[key];
        });

        var values = _(list).map((value: IBuildDefinitionReferenceDto, key: string) => {
            return <h1 key={key}>{value.name}</h1>
        }).value();

        ReactDOM.render(<div>{values}</div>,
            $(".builds")[0]
        );
    }
});

interface IBuildDefinitionReferenceDto {
    name: string;
    id: string;
}