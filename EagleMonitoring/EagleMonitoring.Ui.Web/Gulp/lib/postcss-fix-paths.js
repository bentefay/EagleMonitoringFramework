var traverse = require('traverse');
var postcss = require('postcss');
var path = require('path');
var postcssResult;

// css-modulesify currently has an issue that means it outputs paths of the form C:\C:\<remaining path after options.rootDir> (e.g. C:\C:\Private\app\chart\chart.css)
// This plugin goes through the css and fixes this problem.
var trimPrefix = /([\\/]*[A-Z]:[\\/]*)+/;

module.exports = postcss.plugin('postcss-fix-paths', function (options) {

    return function (css, postcssOptions) {

        postcssResult = postcssOptions;

        // postcssResult.warn("message")

        function rebasePath(filePath) {
            var trimmedPath = filePath.replace(trimPrefix, "");
            var fixedPath = options.rootDir + path.sep + trimmedPath;
            return fixedPath;
        }

        var processed = false;

		traverse(css).forEach(function () {

            // Only process the first input node as each input node is the same object
		    if (processed)
		        return;

		    if (this.key === "input" && this.node.file && this.node.from) {
		        processed = true;
		        this.node.file = rebasePath(this.node.file);
		        this.node.from = rebasePath(this.node.from);
		        this.node.to = options.outputFilePath;
		    }

		});

		postcssOptions.opts.from = rebasePath(postcssOptions.opts.from);
        postcssOptions.opts.to = options.outputFilePath;     
	}
});