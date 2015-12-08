// Configures jquery

// Require the actual jQuery implementation, not our alias
var $ = require('!jquery');
// The inlined plugins below may attach to the jQuery variable
var jQuery = $;

$.noConflict(true);

// To support CORS in IE8 and IE9, we are using xdomainrequest, which has the following restrictions:
// - Only GET or POST
//     When POSTing, the data will always be sent with **NO** Content-Type
// - Only HTTP or HTTPS
//     Protocol must be the same scheme as the calling page
// - Always asynchronous
require("inline:./jquery/jquery.xdomainrequest");

// Validation library
require("inline:./jquery/jquery.validate");

// JQuery UI library for method .scrollParent (http://api.jqueryui.com/category/ui-core/)
require('inline:./jquery-ui/jquery-ui.core');

module.exports = $;