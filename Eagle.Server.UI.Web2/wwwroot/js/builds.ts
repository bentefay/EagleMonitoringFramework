/// <reference path="../typings/tsd.d.ts" />

import DepA = require('./depA');

export module GlobalRoam {

    var z = new DepA();

    var viewModel2 = {
        tabViewModels: ko.observableArray()
    };

    var y = 2;

    ko.applyBindings(viewModel2);

}