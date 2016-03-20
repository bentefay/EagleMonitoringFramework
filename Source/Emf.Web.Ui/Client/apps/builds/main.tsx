/// <reference path="../../typings/all.d.ts"/>

import * as log from "../../common/log";
import { ObservableCollectionManager, IObservableRepositoryEvent } from "../../common/observable-collection-manager";
import $ = require("../../libs/jquery");
import React = require("react");
import ReactDOM = require("react-dom");
import ReactGridLayout = require("../../libs/react-grid-layout");
import "./main.less";

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
            return <div key={key}>{value.name}</div>;
        }).value();

        var x = 0;
        var y = 0;

        var layout = _(list).map((value: IBuildDefinitionReferenceDto, key: string) => {
            var layoutItem: ReactGridLayout.ItemProps = { i: key, x: x, y: y, w: 1, h: 1 };
            x += 1;
            if (x >= 6) {
                x = 0;
                y++;
            }
            return layoutItem;
        }).value();

        ReactDOM.render(<ReactGridLayout layout={layout} cols={6} rowHeight={30}>{values}</ReactGridLayout>,
            $(".builds")[0]
        );
    }
});

interface IBuildDefinitionReferenceDto {
    name: string;
    id: string;
}