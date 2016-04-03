var gulp = require("gulp");
var gutil = require("gulp-util");
var webpack = require("webpack");
var typescriptDefinitions = require('gulp-tsd');
var path = require('path');

var dirs = {};
dirs.root = "./Client/";
dirs.outputRoot = "./public/";
dirs.urlRootPath = "/";
dirs.appsRoot = dirs.root + "apps/";

dirs.appRoot = dirs.appsRoot + "builds/";

var config = {};
config.appEntryPath = dirs.appRoot + "main.tsx";
config.appOutputDirectory = dirs.outputRoot;
config.appOutputFileName = "builds";
config.appFileWatcherGlob = dirs.root + "**/*";
config.typescriptDefinitionsConfigFilePath = dirs.root + "typings.json";

var autoprefixer = require('autoprefixer');
var ExtractTextPlugin = require("extract-text-webpack-plugin");

var webpackConfig = {
    entry: config.appEntryPath,
    output: {
        path: config.appOutputDirectory,
        filename: config.appOutputFileName + ".js",
        publicPath: dirs.urlRootPath // Path to load files from at run time
    },
    resolve: {
        extensions: ["", ".ts", ".js", ".tsx"]
    },
    plugins: [
        new ExtractTextPlugin(config.appOutputFileName + ".css", { disable: false }),
        new webpack.IgnorePlugin(/^\.\/locale$/, /moment$/) // Prevent moment from loading all locales
        // new webpack.ContextReplacementPlugin(/moment[\/\\]locale$/, /de|fr|hu/), // Tell moment to only load the specified locales
    ],
    module: {
        loaders: [
            { test: /\.tsx?$/, loader: "prefix-file-name!ts" },
            { test: /\.js$/, loader: "prefix-file-name" },
            { test: /\.css$/, loader: ExtractTextPlugin.extract("style", "css!postcss") },
            { test: /\.less$/, loader: ExtractTextPlugin.extract("style", "css!postcss!less") },
            { test: /\.png$/, loader: "url?limit=100000&name=[name]-[hash].[ext]" },
            { test: /\.(jpe?g|gif|svg)(\?[\w\d\.=]*?)?$/, loader: "file?name=[name]-[hash].[ext]" },
            { test: /\.(woff|woff2|ttf|eot)(\?[\w\d\.=]*?)?$/, loader: "file?name=[name]-[hash].[ext]" }
        ]
    },
    postcss: function () {
        return [autoprefixer];
    },
    resolveLoader: {
        modulesDirectories: ["node_modules", "WebpackLoaders"]
    }
};

gulp.task("build-dev-watch", ["webpack:build-dev"], function () {
    gulp.watch([config.appFileWatcherGlob], ["webpack:build-dev"]);
});

gulp.task("build-prod", ["webpack:build-prod"]);

gulp.task("webpack:build-prod", function (callback) {
    
    var productionWebpackConfig = Object.create(webpackConfig);
    productionWebpackConfig.output.filename = changeExtension(productionWebpackConfig.output.filename, ".min.js");
    productionWebpackConfig.plugins = productionWebpackConfig.plugins.concat(
		new webpack.DefinePlugin({ "process.env": { "NODE_ENV": JSON.stringify("production") } }),
		new webpack.optimize.DedupePlugin(),
		new webpack.optimize.UglifyJsPlugin()
	);

    webpack(productionWebpackConfig, function (err, stats) {
        onBuildCompleted("webpack:build", err, stats);
        callback();
    });
});

function changeExtension(filePath, newExtension) {
    return filePath.replace(/\..*?$/, function () {
        return newExtension;
    });
}

var webpackDevConfig = Object.create(webpackConfig);
webpackDevConfig.debug = true;

var devCompiler = webpack(webpackDevConfig);

gulp.task("webpack:build-dev", function (callback) {
    devCompiler.run(function (err, stats) {
        onBuildCompleted("webpack:build-dev", err, stats);
        callback();
    });
});

function onBuildCompleted(buildName, err, stats) {
    if (err) throw new gutil.PluginError(buildName, err);
    gutil.log("[" + buildName + "]", stats.toString({
        colors: true
    }));
    if (stats.compilation.errors && stats.compilation.errors.length) {
        gutil.log(gutil.colors.red("Build failed"));
    } else {
        gutil.log(gutil.colors.green("Build completed succcessfully"));
    }
}

var gulpTypings = require("gulp-typings");
gulp.task("ts:download-definitions", function () {
    gulp.src(config.typescriptDefinitionsConfigFilePath).pipe(gulpTypings());
});