// Signalr currently expects that if you are using jquery, it exists on the window global. 
// We need to temporarily add jquery back to the window so that it can bind to it.

var win = <any>window;

var oldJQuery = win.jQuery;

var newJQuery = <JQueryStatic>require('jquery');

win.jQuery = newJQuery;

var signalr = require('ms-signalr-client');

require("inline:./signalr-hubs.js");

win.jQuery = oldJQuery;

export = signalr;