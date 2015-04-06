$(function () {

    var viewModel = {
        descriptions: ko.observableArray()
    };

    var productMonitorHub = $.connection.productMonitorHub;

    productMonitorHub.client.broadcastMessage = function (name, message) {
        viewModel.descriptions.push({ name: name, message: message });
    }

    $.connection.hub.start().done();

    ko.applyBindings(viewModel);

});