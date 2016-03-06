/// <reference path="../typings/all.d.ts" />

let ReactGridLayout = require("react-grid-layout") as any;

ReactGridLayout = ReactGridLayout.WidthProvider(ReactGridLayout);

const TypedReactGridLayout = ReactGridLayout as ReactGridLayout;

export = TypedReactGridLayout;