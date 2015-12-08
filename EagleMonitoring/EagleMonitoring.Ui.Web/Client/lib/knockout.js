// Set up knockout.

// Knockout currently expects that if you are using jquery, it exists on the window global. 
// We need to temporarily add jquery back to the window so that knockout can bind to it.

var oldJQuery = window.jQuery;
var oldKo = window.ko;

window.jQuery = require('jquery');

var knockout = require('!knockout');

window.ko = knockout;

// ES5 shim is required by knockout-transformations to shim methods including Array.isArray and others
require('es5-shim');

require('knockout-transformations');

window.jQuery = oldJQuery;
window.ko = oldKo;

module.exports = knockout;