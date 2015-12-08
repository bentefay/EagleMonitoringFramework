/// <binding Clean='clean' />
"use strict";

var gulp = require("gulp");
var gutil = require("gulp-util");
var webpack = require("webpack");
var mergeStreams = require('merge-stream');
var gutil = require('gulp-util');
var tsd = require('gulp-tsd');
var ts = require('gulp-typescript');
var concat = require('gulp-concat');
var insert = require('gulp-insert');
var browserify = require('browserify');
var watchify = require('watchify');
var source = require('vinyl-source-stream');
var cssSelectorLimit = require('gulp-css-selector-limit');
var cssPrefix = require('./Gulp/gulp-css-prefix');
var less = require("gulp-less");
var LessAutoprefix = require("less-plugin-autoprefix");
var path = require('path');
var Core = require('css-modules-loader-core');
var rimraf = require('rimraf');


// ---- VARIABLES

var paths = {
    webRoot: "./wwwroot/",
    source: "./Client/"
};

paths.js = paths.webRoot + "**/*.js";

gulp.task('ts-download-definitions', function (callback) {
    tsd({
        command: 'reinstall',
        config: paths.srcroot + 'tsd.json'
    }, callback);
});

gulp.task('libraries-build-css', function () {
    return mergeStreams(
            createCssBuild(
            {
                lessPath: 'lib/bootstrap/bootstrap.less',
                intermediateCssPath: 'lib/bootstrap',
                outputCssPath: '.',
                css: {
                    identifiersOutsideParent: [".modal-open", ".modal-scrollbar-measure", ".modal-backdrop"]
                }
            }),
            createCssBuild(
            {
                lessPath: 'lib/font-awesome/font-awesome.less',
                intermediateCssPath: 'lib/font-awesome',
                outputCssPath: '.',
                css: {
                    identifiersOutsideParent: []
                }
            }))
        .pipe(insert.transform(function(contents, file) {

            var comment = '/* FILE: ' + path.basename(file.path) + ' */\n';
            return comment + contents;
        }))
        .pipe(concat({ path: 'app-lib.css' }))
        .pipe(gulp.dest(paths.browserifyOutput))
        .pipe(cssSelectorLimit())
        .pipe(cssSelectorLimit.reporter('default'))
        .pipe(cssSelectorLimit.reporter('fail'));
});

function createCssBuild(options) {
    return gulp.src(paths.srcroot + options.lessPath)
        .pipe(less({ plugins: [lessAutoprefix] }))
        .pipe(gulp.dest(paths.srcroot + options.intermediateCssPath))
        .pipe(cssPrefix({
            insertParent: ".gr",
            identifiersOutsideParent: options.css.identifiersOutsideParent,
            identifiersToDelete: ["html", "body"],
            insertClassPrefix: "gr-",
            insertIdPrefix: "gr-"
        }))
        .pipe(gulp.dest(paths.browserifyOutput + options.outputCssPath));
}

gulp.task('check-css-files-selector-limit', function () {
    return gulp.src(paths.browserifyOutput + '**/*.css')
        .pipe(cssSelectorLimit())
        .pipe(cssSelectorLimit.reporter('default'))
        .pipe(cssSelectorLimit.reporter('fail'));
});

var tsProject = ts.createProject({
    declarationFiles: false,
    noExternalResolve: false,
    module: 'CommonJS',
    removeComments: true
});

gulp.task('clean', function (cb) {
    rimraf(paths.browserifyOutput, cb);
});

gulp.task('app-ts-compile', function () {
    var tsResult = gulp.src(paths.typescriptFiles, { base: "." })
        .pipe(ts(tsProject));

    return tsResult.js.pipe(gulp.dest("."));
});