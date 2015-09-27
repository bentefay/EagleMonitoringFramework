/// <reference path="../typings/tsd.d.ts" />
var DepA = require('./depA');
var GlobalRoam;
(function (GlobalRoam) {
    var z = new DepA();
    var viewModel2 = {
        tabViewModels: ko.observableArray()
    };
    var y = 2;
    ko.applyBindings(viewModel2);
})(GlobalRoam = exports.GlobalRoam || (exports.GlobalRoam = {}));
