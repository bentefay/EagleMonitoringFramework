// Prefixes bundled javascript files with a comment containing the file name of the bundled file

var gutil = require('gulp-util'),
    transformTools = require("browserify-transform-tools"),
    path = require('path');

module.exports = transformTools.makeStringTransform("browserify-prefix-filename-header", {}, // includeExtensions: [".js"]
    function(content, transformOptions, done) {
        // gutil.log("Adding file name header to " + transformOptions.file);
        done(null, "// " + path.basename(transformOptions.file) + "\n" + content);
    });