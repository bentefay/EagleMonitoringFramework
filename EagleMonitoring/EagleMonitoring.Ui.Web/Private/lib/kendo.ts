// Sets up kendo. Currently kendo unavoidably:
// - pollutes the global namespace with a "kendo" variable
// - attaches itself to jquery
// - adds a class to the html body element that can be used for determining the browser type and version (e.g. class="ie8")

// Set up jQuery aliases in this closure to ensure kendo attaches to the correct instance of jquery
var $ = require('jquery');
var jQuery = $;

// We are inlining kendo here so that it will bind to the above jquery instance
require('inline:./kendo/kendo.core.drawing.min');

export = kendo;