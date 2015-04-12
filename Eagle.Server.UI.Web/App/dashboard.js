$(function () {

    var viewModel = {
        tabViewModels: ko.observableArray([
            /*
            {
                tabName: ko.observable(),
                columnHeaders: ko.observableArray([
                    {
                        location: ""
                    }
                ]),
                rows: ko.observableArray([
                    {
                        checkType: "",
                        cells: ko.observableArray([
                            {
                                status: ko.observable(),
                                tooltipContent: ko.observable(),
                                content: ko.observable()
                            }
                        ])
                    }
                ])
            }
            */
        ])
    };

    var productMonitorHub = $.connection.productMonitorHub;

    productMonitorHub.client.updateChecks = updateChecks;

    $.connection.hub.start().done(onConnectionEstablished);

    ko.applyBindings(viewModel);

    function onConnectionEstablished() {

        productMonitorHub.server.getChecks().done(updateChecks);
    }

    function updateChecks(checks) {
        Enumerable.From(checks).ForEach(updateCheck);
    }

    function updateCheck(check) {

        var tabViewModel = getOrAdd(viewModel.tabViewModels,
            function(tab) { return tab.tabName === check.tabName; },
            function() { return { tabName: check.tabName, rows: ko.observableArray(), columnHeaders: ko.observableArray() }; });

        var columnIndex = _(tabViewModel.columnHeaders()).findIndex(function(columnHeader) { return columnHeader.location === check.location; });
        if (columnIndex == -1) {
            Enumerable.From(tabViewModel.rows()).ForEach(function(row) { row.cells.push(createEmptyCell()); });
            tabViewModel.columnHeaders.push({ location: check.location });
            columnIndex = tabViewModel.columnHeaders().length - 1;
        };

        var rowViewModel = getOrAdd(tabViewModel.rows,
            function(row) { return row.checkType === check.checkType },
            function() {
                var columns = Enumerable.From(tabViewModel.columnHeaders()).Select(createEmptyCell).ToArray();
                return { checkType: check.checkType, cells: ko.observableArray(columns) };
            });

        var cellViewModel = rowViewModel.cells()[columnIndex];

        updateCellViewModel(cellViewModel, check);
    }

    function createEmptyCell() {
        return {
            status: ko.observable('notApplicable'),
            tooltipContent: ko.observable("Not Applicable"),
            content: ko.observable("N/A")
        };
    }

    function getOrAdd(observableArray, predicate, factory) {
        var values = Enumerable.From(observableArray()).Where(predicate).ToArray();

        if (values.length == 0) {
            var value = factory();
            observableArray.push(value);
            return value;
        } else {
            return values[0];
        }
    }

    function updateCellViewModel(cellViewModel, check) {

        var state = getCellViewModelState(check);

        cellViewModel.status(state.status);
        cellViewModel.tooltipContent(state.tooltipContent);
        cellViewModel.content(check.result);
    }

    function getCellViewModelState(check) {
        if (check.isTriggered)
            return { status: 'triggered', tooltipContent: check.status };
        else if (check.hasError)
            return { status: 'errored', tooltipContent: check.error };
        else if (check.isPaused)
            return { status: 'paused', tooltipContent: check.status };
        else if (check.isLoading)
            return { status: 'loading', tooltipContent: check.status };
        else
            return { status: 'normal', tooltipContent: check.status };
    }

});