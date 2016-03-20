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

class MainComponent {

    constructor() {

        var manager = new ObservableCollectionManager("./signalr", { clearError: () => { }, showError: message => { } });

        var buildStates = new BuildStateCollection();

        manager.subscribe<IBuildDefinitionReference>("buildDefinitions", {
            onNewEvent: event => {
                _.forEach(event.newOrUpdatedItems, buildDefinition => {
                    const buildState = buildStates.get(buildDefinition.key);
                    buildState.definition = buildDefinition.value;
                });

                _.forEach(event.deletedItemKeys, key => {
                    buildStates.deleteDefinition(key);
                });

                var values = _(buildStates.map).map((value: IBuildDefinitionReference, key: string) => {
                    return <div key={key}>{value.name}</div>;
                }).value();

                var x = 0;
                var y = 0;

                var layout = _(buildStates.map).map((value: IBuildDefinitionReference, key: string) => {
                    var layoutItem: ReactGridLayout.ItemProps = { i: key, x: x, y: y, w: 1, h: 1 };
                    x += 1;
                    if (x >= 6) {
                        x = 0;
                        y++;
                    }
                    return layoutItem;
                }).value();

                this.render(layout, values);
            }
        });

        manager.subscribe<IBuild>("builds", {
            onNewEvent: event => {

            }
        });
    }

    render(layout: ReactGridLayout.ItemProps[], values: JSX.Element[]) {
        ReactDOM.render(<ReactGridLayout layout={layout} cols={6} rowHeight={30}>{values}</ReactGridLayout>,
            $(".builds")[0]
        );
    }

    
}

var mainComponent = new MainComponent();

class BuildStateCollection {

    map: { [buildDefinitionId: string]: BuildState } = {};

    get(buildDefinitionId: string) : BuildState {
        var value = this.map[buildDefinitionId];
        if (value) {
            return value;
        } else {
            this.map[buildDefinitionId] = value = new BuildState();
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
            }
        }
    }
}

class BuildState {
    definition: IBuildDefinitionReference;
    latestBuild: IBuild;
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