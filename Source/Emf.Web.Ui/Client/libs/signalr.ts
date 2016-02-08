/// <reference path="../typings/all.d.ts" />

import newJQuery = require("./jquery");

var win = <any>window;

var oldJQuery = win.jQuery;
win.jQuery = newJQuery;

var signalr = require("ms-signalr-client");
import "./signalr/hubs";

win.jQuery = oldJQuery;

export = signalr;