var gulp = require("gulp");
var gutil = require("gulp-util");
var webpack = require("webpack");
var typescriptDefinitions = require('gulp-tsd');
var path = require('path');

var dirs = {};
dirs.root = "./Client/";
dirs.appsRoot = dirs.root + "apps/";
dirs.outputRoot = "./public/";

dirs.appRoot = dirs.appsRoot + "builds/";

var config = {};
config.appEntryPath = dirs.appRoot + "main.tsx";
config.appOutputDirectory = dirs.outputRoot;
config.appOutputFileName = "builds.js";
config.appFileWatcherGlob = dirs.root + "**/*";
config.typescriptDefinitionsConfigFilePath = dirs.root + "tsd.json";

var autoprefixer = require('autoprefixer');
var precss = require('precss');

var webpackConfig = {
    entry: config.appEntryPath,
    output: {
        path: config.appOutputDirectory,
        filename: config.appOutputFileName
    },
    resolve: {
        extensions: ["", ".ts", ".js", ".tsx"]
    },
    plugins: [],
    module: {
        loaders: [
            { test: /\.tsx?$/, loader: "prefix-file-name-loader!ts-loader" },
            { test: /\.css$/, loader: "style-loader!css-loader?modules!postcss-loader" },
            { test: /\.less$/, loader: "style-loader!css-loader?modules!postcss-loader!less-loader" },
            { test: /\.png$/, loader: "url-loader?limit=100000" },
            { test: /\.jpg$/, loader: "file-loader" }
        ]
    },
    postcss: function () {
        return [autoprefixer, precss];
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
        if (err) throw new gutil.PluginError("webpack:build", err);
        gutil.log("[webpack:build]", stats.toString({
            colors: true
        }));
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
        if (err) throw new gutil.PluginError("webpack:build-dev", err);
        gutil.log("[webpack:build-dev]", stats.toString({
            colors: true
        }));
        callback();
    });
});

gulp.task('ts:download-definitions', function (callback) {
    typescriptDefinitions({
        command: 'reinstall',
        config: config.typescriptDefinitionsConfigFilePath
    }, callback);
});