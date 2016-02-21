/// <reference path="../../typings/all.d.ts"/>

import $ = require("../../libs/jquery");
import _ = require("lodash");
var log = require<any>("../../libs/log");
var x = require("./observable");

console.log(x);
console.log(_.contains(['bye'], 'bye'));

// log.error("!");

//import { ObservableCollectionManager, IObservableRepositoryEvent } from "../../libs/observable-collection-manager";

//var manager = new ObservableCollectionManager("./signalr", { clearError: () => { }, showError: message => { } });

//manager.subscribe("buildDefinitionReferences", {
//    onNewEvent: event => {
//        _.forEach(event.newOrUpdatedItems, item => {
//            document.write(item.value);
//        });
//    }
//});