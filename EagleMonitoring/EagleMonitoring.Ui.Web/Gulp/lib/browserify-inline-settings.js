// Directly inlines the contents of a file (without wrapping it). Converts requires of the form require("inline:<filePath>");

var gutil = require('gulp-util'),
    path = require('path'),
    transformTools = require("browserify-transform-tools");

var jsonRegex = /^setting:\s*(.*)$/;

var transformName = "browserify-inline-settings";

module.exports = transformTools.makeRequireTransform(transformName,
    { evaluateArguments: true, jsFilesOnly: true },
    function (args, opts, cb) {

        var settings = opts.config.settings;

        var jsonFile = args[0];

        var match = jsonRegex.exec(jsonFile);

        if (match) {

            var settingKey = match[1];

            if (settingKey in settings) {

                var setting = settings[settingKey];
                gutil.log("Inserting setting with key " + settingKey + " into " + path.basename(opts.file));
                return cb(null, JSON.stringify(setting));

            } else {
                throw new gutil.PluginError({
                    plugin: transformName,
                    message: gutil.colors.red("Could not find setting with key '" + settingKey + "' required in file " + path.basename(opts.file) + ". Available keys: " + JSON.stringify(settings))
                });
            }
        } else {
            return cb();
        }
    });