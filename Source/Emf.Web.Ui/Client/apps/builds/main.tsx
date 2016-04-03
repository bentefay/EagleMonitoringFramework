/// <reference path="../../typings/all.d.ts"/>

import * as log from "../../common/log";
import { ObservableCollectionManager, IObservableRepositoryEvent } from "../../common/observable-collection-manager";
import _ = require("lodash");
import $ = require("../../libs/jquery");
import React = require("react");
import ReactDOM = require("react-dom");
import ReactGridLayout = require("../../libs/react-grid-layout");
import "./main.less";

log.logger.setLogLevel(log.LogLevel.Debug);
log.logger.logEvents.subscribe(new log.ConsoleObserver());

class BuildStateCollection {

    map: { [buildDefinitionId: string]: BuildState } = {};
    count = 0;

    get(buildDefinitionId: string): BuildState {
        var value = this.map[buildDefinitionId];
        if (value) {
            return value;
        } else {
            this.map[buildDefinitionId] = value = new BuildState();
            this.count++;
            return value;
        }
    }

    deleteDefinition(buildDefinitionId: string) {
        this._delete(buildDefinitionId, "definition");
    }

    deleteLatestBuild(buildDefinitionId: string) {
        this._delete(buildDefinitionId, "latestBuild");
    }

    private _delete(buildDefinitionId: string, path: string) {
        var value = this.map[buildDefinitionId];
        if (value) {
            value[path] = null;
            if (!value.definition && !value.latestBuild) {
                delete this.map[buildDefinitionId];
                this.count--;
            }
        }
    }
}

class MainComponent {

    buildStates: BuildStateCollection;
    manager: ObservableCollectionManager;


    constructor() {

        this.manager = new ObservableCollectionManager("./signalr", { clearError: () => { }, showError: message => { } });
        this.buildStates = new BuildStateCollection();

        this.manager.subscribe<IBuildDefinitionReference>("buildDefinitions", {
            onNewEvent: event => {
                _.forEach(event.newOrUpdatedItems, buildDefinition => {
                    const buildState = this.buildStates.get(buildDefinition.key);
                    buildState.definition = buildDefinition.value;
                });

                _.forEach(event.deletedItemKeys, key => {
                    this.buildStates.deleteDefinition(key);
                });

                this.render();
            }
        });

        this.manager.subscribe<IBuild>("builds", {
            onNewEvent: event => {
                _.forEach(event.newOrUpdatedItems, build => {
                    const buildState = this.buildStates.get(build.key);
                    buildState.latestBuild = build.value;
                });

                _.forEach(event.deletedItemKeys, key => {
                    this.buildStates.deleteLatestBuild(key);
                });

                this.render();
            }
        });
    }

    onLayoutChanged(itemProps: ReactGridLayout.ItemProps[]) {
        
    }

    render = () => {

        const buildStates = _(this.buildStates.map)
            .map((value: BuildState) => value)
            .filter((value: BuildState) => value.definition)
            .orderBy((value: BuildState) => value.definition.name)
            .value();

        var values = _.map(buildStates, buildState => {
            return <div key={buildState.definition.id} style={{ backgroundColor: this.getBuildStateColor(buildState) }}>
                <span>{buildState.definition.name}</span>
                {(() => {
                    if (buildState.latestBuild && buildState.latestBuild.testRuns.length > 0) {
                        var firstRun = buildState.latestBuild.testRuns[0];
                        return <span style={{ marginLeft: '4px' }}>{firstRun.passedTests}/{firstRun.totalTests}</span>;
                    }
                })() }
            </div>;
        });

        var x = 0;
        var y = 0;

        var layout = _.map(buildStates, buildState => {
            var layoutItem: ReactGridLayout.ItemProps = { i: buildState.definition.id.toString(), x: x, y: y, w: 1, h: 1 };
            x += 1;
            if (x >= 6) {
                x = 0;
                y++;
            }
            return layoutItem;
        });

        ReactDOM.render(<ReactGridLayout layout={layout} cols={6} rowHeight={30} onLayoutChange={this.onLayoutChanged}>{values}</ReactGridLayout>,
            $(".builds")[0]
        );
    }

    getBuildStateColor(buildState: BuildState) {

        if (!buildState.latestBuild)
            return "#F7F7F9";

        switch (buildState.latestBuild.result) {
            case BuildResult.None:
                return "#F7F7F9";
            case BuildResult.Succeeded:
                return "#5CB85C";
            case BuildResult.PartiallySucceeded:
                return "#F0AD4E";
            case BuildResult.Failed:
                return "#D9534F";
            case BuildResult.Canceled:
                return "#5BC0DE";
        }
    }
}

var mainComponent = new MainComponent();

class BuildState {

    constructor() {
        this.viewModel = { order: 0, width: 1, height: 1 };
    }

    definition: IBuildDefinitionReference;
    latestBuild: IBuild;
    viewModel: IBuildStateViewModel;
}

interface IBuildStateViewModel {
    order: number;
    width: number;
    height: number;
}

interface IBuildDefinitionReference {
    id: number;
    revision: number;
    name: string;
    type: DefinitionType;
}

interface IBuild {

    id: number;
    definition: IBuildDefinitionReference;

    status: BuildStatus;

    queueTime: string;
    startTime: string;
    finishTime: string;

    result: BuildResult;

    testRuns: ITestRun[];
}

interface ITestRun {
    id: number;
    incompleteTests: number;
    passedTests: number;
    notApplicableTests: number;
    totalTests: number;
    errorMessages: string;
}

enum DefinitionType {
    Xaml = 1,
    Build = 2
}

enum BuildStatus {
    None = 0,
    InProgress = 1,
    Completed = 2,
    Cancelling = 4,
    Postponed = 8,
    NotStarted = 32,
    All = 47
}

enum BuildResult {
    None = 0,
    Succeeded = 2,
    PartiallySucceeded = 4,
    Failed = 8,
    Canceled = 32,
}