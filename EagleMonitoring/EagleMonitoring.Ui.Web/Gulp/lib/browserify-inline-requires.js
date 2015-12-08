// Directly inlines the contents of a file (without wrapping it). Converts requires of the form require("inline:<filePath>");

var gutil = require('gulp-util'),
    transformTools = require("browserify-transform-tools"),
    resolve = require("resolve"),
    path = require('path'),
    syncFs = require('sync-fs');

module.exports = transformTools.makeRequireTransform("browserify-inline-requires",
    { evaluateArguments: true, jsFilesOnly: true },
    function(args, opts, cb) {
        var find = "inline:";
        if (args[0].indexOf(find) == 0) {
            var filePath = args[0].substr(find.length);
            var baseDir = path.dirname(opts.file);
            // gutil.log("Inlining " + baseDir + "\\" + filePath);
            var fullPath = resolve.sync(filePath, { basedir: baseDir });
            var res = syncFs.readFile(fullPath, 'utf8');
            return cb(null, res);
        } else {
            return cb();
        }
    });