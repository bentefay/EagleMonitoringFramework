// Use with aliasify. Prefix those requires you do not want to be aliased with a custom identifier, run aliasify, then run this transform to strip the prefix.

var gutil = require('gulp-util'),
    transformTools = require("browserify-transform-tools");

module.exports = transformTools.makeRequireTransform("browserify-strip-requires-prefix",
        { evaluateArguments: true, jsFilesOnly: true },
        function (args, options, cb) {

            var prefix = options.config.prefix;
            var newRequire = args[0];

            if (newRequire.indexOf(prefix) === 0) {
                // gutil.log("Stripping prefix from " + newRequire);
                newRequire = newRequire.substr(prefix.length);
            }

            cb(null, "require('" + newRequire + "')");
        });