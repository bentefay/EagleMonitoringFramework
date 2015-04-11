$(function () {

    var viewModel = {
        descriptions: ko.observableArray()
    };

    var productMonitorHub = $.connection.productMonitorHub;

    productMonitorHub.client.updateChecks = updateChecks;

    $.connection.hub.start().done(onConnectionEstablished);

    ko.applyBindings(viewModel);

    function onConnectionEstablished() {

        productMonitorHub.server.getChecks().done(updateChecks);
    }

    function updateChecks(checks) {

        console.log(JSON.stringify(checks));

    }

});