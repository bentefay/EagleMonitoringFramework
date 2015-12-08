/// <binding Clean='clean' />
"use strict";

var gulp = require("gulp");
var gutil = require("gulp-util");
var webpack = require("webpack");

// ---- VARIABLES

var paths = {
    webRoot: "./wwwroot/",
    source: "./Client/"
};

paths.js = paths.webRoot + "**/*.js";


function defineApp() {

    var webpackConfig = {
        entry: './app.ts',
        output: {
            filename: 'bundle.js'
        },
        resolve: {
            // Add `.ts` and `.tsx` as a resolvable extension.
            extensions: ['', '.webpack.js', '.web.js', '.ts', '.tsx', '.js']
        },
        module: {
            loaders: [
                // all files with a `.ts` or `.tsx` extension will be handled by `ts-loader`
                { test: /\.tsx?$/, loader: 'ts-loader' }
            ]
        }
    }

// ---- BUILD DEFINITIONS

    gulp.task("build-dev", ["webpack:build-dev"], function() {
        gulp.watch(["app/**/*"], ["webpack:build-dev"]);
    });

// Production build
    gulp.task("build", ["webpack:build"]);

    gulp.task("webpack:build", function(callback) {
        // modify some webpack config options
        var myConfig = Object.create(webpackConfig);
        myConfig.plugins = myConfig.plugins.concat(
            new webpack.DefinePlugin({
                "process.env": {
                    // This has effect on the react lib size
                    "NODE_ENV": JSON.stringify("production")
                }
            }),
            new webpack.optimize.DedupePlugin(),
            new webpack.optimize.UglifyJsPlugin()
        );

        // run webpack
        webpack(myConfig, function(err, stats) {
            if (err) throw new gutil.PluginError("webpack:build", err);
            gutil.log("[webpack:build]", stats.toString({
                colors: true
            }));
            callback();
        });
    });

// modify some webpack config options
    var myDevConfig = Object.create(webpackConfig);
    myDevConfig.devtool = "sourcemap";
    myDevConfig.debug = true;

// create a single instance of the compiler to allow caching
    var devCompiler = webpack(myDevConfig);

    gulp.task("webpack:build-dev", function(callback) {
        // run webpack
        devCompiler.run(function(err, stats) {
            if (err) throw new gutil.PluginError("webpack:build-dev", err);
            gutil.log("[webpack:build-dev]", stats.toString({
                colors: true
            }));
            callback();
        });
    });

}