/// <reference path="../../typings/all.d.ts"/>

import * as log from "../../common/log";
import { ObservableCollectionManager, IObservableRepositoryEvent } from "../../common/observable-collection-manager";
import _ = require("lodash");
import $ = require("../../libs/jquery");
import React = require("react");
import ReactDOM = require("react-dom");
import ReactGridLayout = require("../../libs/react-grid-layout");
import "./main.less";
import "font-awesome/css/font-awesome.min.css";

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
            .filter((value: BuildState) => value.definition && !_.startsWith(value.definition.name, "OLD_") && !_.endsWith(value.definition.name, "_Deprecated"))
            .orderBy((value: BuildState) => value.definition.name)
            .value();

        const groupedBuildStates = _(buildStates).groupBy(s => this.getProjectName(s.definition.name))
            .map((buildStates: BuildState[], key: string) => { return { buildStates, key }; })
            .orderBy(b => b.key)
            .value();

        const values = _.map(groupedBuildStates, group => {
            return this.getProjectComponent(group.buildStates, group.key);
        });

        const columnCount = 5;
        const columnHeights = _.map(_.range(0, columnCount), columnIndex => { return { columnIndex, height: 0 }; });

        const layout = _.map(groupedBuildStates, group => {

            var minHeightColumn = _.minBy(columnHeights, c => c.height);
            var elementHeight = 1;

            var layoutItem: ReactGridLayout.ItemProps = {
                i: group.key,
                x: minHeightColumn.columnIndex, y: minHeightColumn.height, w: 1, h: elementHeight
            };

            minHeightColumn.height += elementHeight;

            return layoutItem;
        });

        ReactDOM.render(<ReactGridLayout layout={layout} cols={columnCount} rowHeight={30} onLayoutChange={this.onLayoutChanged}>{values}</ReactGridLayout>,
            $(".builds")[0]
        );
    }

    getProjectComponent(buildStates: BuildState[], key: string) {
        return <div key={key} style={{ backgroundColor: this.getProjectStateColor(buildStates) }}>
            <div style={{ position: "absolute", left: "10px", right: "10px", top: "0", bottom: "0", lineHeight: "30px" }}>

                <div style={{ textOverflow: "ellipsis", overflow: "hidden", verticalAlign: "middle", whiteSpace: "nowrap", textAlign: "right", float: "right" }}>
                    {_.map(buildStates, buildState => this.getProjectBuildComponent(buildState)) }
                </div>

                <div style={{ textOverflow: "ellipsis", overflow: "hidden", verticalAlign: "middle", whiteSpace: "nowrap", textAlign: "left", float: "left" }}>
                    <span style={{ fontWeight: "bold", fontSize: "0.8em" }}>{key}</span>
                </div>

            </div>
        </div>;
    }

    getProjectBuildComponent(buildState: BuildState) {
        return <span key={buildState.definition.id} className="project-build"
            style={{ backgroundColor: this.getBuildStateColor(buildState), margin: "0 0 0 5px", padding: "4px", borderRadius: "2px" }}
            title={buildState.definition.name}>
            {this.getProjectBuildIconComponent(this.getProjectBuildName(buildState.definition.name)) }{this.getTestsComponent(buildState) }
        </span>;
    }

    getProjectBuildIconComponent(name: string) {

        var icon = this.getProjectBuildIcon(name);

        if (icon) {
            return <i className={`fa fa-${icon}`}></i>;
        } else {
            return <span>{name}</span>;
        }
    }

    getProjectBuildIcon(name: string) {
        switch (name) {
            case "Release":
                return "star";
            case "N":
                return "moon-o";
            case "CI":
                return "sun-o";
            case "N_UI":
                return "desktop";
        }
    }

    getTestsComponent(buildState: BuildState) {
        if (buildState.latestBuild && buildState.latestBuild.testRuns.length > 0) {
            return _(buildState.latestBuild.testRuns)
                .filter(testRun => testRun.passedTests + testRun.notApplicableTests !== testRun.totalTests)
                .map(testRun => {
                    return <span key={testRun.id} style={{ marginLeft: '4px' }}>{testRun.passedTests}/{testRun.totalTests}</span>;
                })
                .value();
        } else {
            return null;
        }
    }

    getProjectBuildName(name: string) {
        const buildName = this.substringFromLast(name, ".");
        const buildNamePostfix = this.substringFromFirst(buildName, "_");
        if (buildName === buildNamePostfix)
            return "Release";
        return buildNamePostfix;
    }

    getProjectName(name: string) {
        const projectNameStart = _.lastIndexOf(name, ".");
        if (projectNameStart === -1)
            return name;
        const buildNamePostfixStart = _.indexOf(name, "_", projectNameStart);
        if (buildNamePostfixStart === -1)
            return name;
        return name.substring(0, buildNamePostfixStart);
    }

    substringFromFirst(str: string, separator: string) {
        const index = _.indexOf(str, separator);
        return index === -1 ? str : str.substring(index + 1);
    }

    substringFromLast(str: string, separator: string) {
        const index = _.lastIndexOf(str, separator);
        return index === -1 ? str : str.substring(index + 1);
    }

    getProjectStateColor(buildStates: BuildState[]) {

        const buildStatesWithResults = _.filter(buildStates, b => b.latestBuild && b.latestBuild.result);

        let worstResult = { result: BuildResult.None };

        if (_.some(buildStatesWithResults)) {
            worstResult = _(buildStatesWithResults)
                .map(b => b.latestBuild.result)
                .map(r => { return { alertLevel: this.getBuildResultAlertLevel(r), result: r }; })
                .maxBy(r => r.alertLevel);
        }

        const color = this.getBuildResultColor(worstResult.result);

        return Color.parseColor(color).mix(Color.white, 0.5).toString();
    }

    getBuildResultAlertLevel(result: BuildResult) {
        switch (result) {
            case BuildResult.None:
                return 0;
            case BuildResult.Succeeded:
                return 2;
            case BuildResult.PartiallySucceeded:
                return 3;
            case BuildResult.Failed:
                return 4;
            case BuildResult.Canceled:
                return 1;
        }
    }

    getBuildStateColor(buildState: BuildState) {

        if (!buildState.latestBuild)
            return this.getBuildResultColor(BuildResult.None);

        return this.getBuildResultColor(buildState.latestBuild.result);
    }

    getBuildResultColor(result: BuildResult) {
        switch (result) {
            case BuildResult.None:
                return "#5CB85C"; // F7F7F9
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

class Color {

    static white = new Color(255, 255, 255);
    static black = new Color(0, 0, 0);

    constructor(public red: number, public green: number, public blue: number) { }

    static parseColor(color: string): Color {
        const m = color.match(/^#([0-9a-f]{6})$/i)[1];
        if (m) {
            return new Color(
                parseInt(m.substr(0, 2), 16),
                parseInt(m.substr(2, 2), 16),
                parseInt(m.substr(4, 2), 16)
            );
        }
        return new Color(0, 0, 0);
    }

    mix(color: Color, newColorWeighting: number): Color {
        const n = newColorWeighting;
        const o = 1 - n;
        return new Color(o * this.red + n * color.red, o * this.green + n * color.green, o * this.blue + n * color.blue);
    }

    toString() {
        const str = "#" + Math.round(this.red).toString(16) + Math.round(this.green).toString(16) + Math.round(this.blue).toString(16);
        return str;
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