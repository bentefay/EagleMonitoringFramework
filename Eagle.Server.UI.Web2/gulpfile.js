/// <binding Clean='clean' />
var gulp = require("gulp"),
    rimraf = require("rimraf"),
    concat = require("gulp-concat"),
    cssmin = require("gulp-cssmin"),
    uglify = require("gulp-uglify"),
    gutil = require('gulp-util'),
    source = require('vinyl-source-stream'),
    browserify = require('browserify'),
    watchify = require('watchify'),
    tsd = require('gulp-tsd'),
    ts = require('gulp-typescript'),
    project = require("./project.json");

var paths = {
    webroot: "./" + project.webroot + "/"
};

paths.appRoots = [ paths.webroot + "js/builds.js" ];
paths.js = paths.webroot + "js/**/*.js";
paths.minJs = paths.webroot + "js/**/*.min.js";
paths.css = paths.webroot + "css/**/*.css";
paths.minCss = paths.webroot + "css/**/*.min.css";
paths.concatJsDest = paths.webroot + "js/site.min.js";
paths.concatCssDest = paths.webroot + "css/site.min.css";
paths.ts = paths.webroot + "js/**/*.ts";
paths.browserifyDest = paths.webroot + "js";

//gulp.task("clean:js", function (cb) {
//    rimraf(paths.concatJsDest, cb);
//});

gulp.task("clean:css", function (cb) {
    rimraf(paths.concatCssDest, cb);
});

gulp.task("clean", ["clean:css"]);

//gulp.task("min:js", function () {
//    gulp.src([paths.js, "!" + paths.minJs], { base: "." })
//        .pipe(concat(paths.concatJsDest))
//        .pipe(uglify())
//        .pipe(gulp.dest("."));
//});

gulp.task("min:css", function () {
    gulp.src([paths.css, "!" + paths.minCss])
        .pipe(concat(paths.concatCssDest))
        .pipe(cssmin())
        .pipe(gulp.dest("."));
});

gulp.task("min", ["min:js", "min:css"]);

gulp.task('tsd', function (callback) {
    tsd({
        command: 'reinstall',
        config: './tsd.json'
    }, callback);
});

var tsProject = ts.createProject({
    declarationFiles: false,
    noExternalResolve: false,
    module: 'CommonJS',
    removeComments: true
});

gulp.task('ts-compile', function () {
    var tsResult = gulp.src(paths.ts, { base: "." })
        .pipe(ts(tsProject));

    return tsResult.js.pipe(gulp.dest("."));
});

gulp.task('watch', ['ts-compile'], function () {
    gulp.watch(paths.ts, ['ts-compile']);
});

var b = watchify(browserify({
    entries: paths.appRoots,
    debug: false
}));

gulp.task('browserify', bundle);
b.on('update', bundle); // on any dep update, runs the bundler
b.on('log', gutil.log); // output build logs to terminal

function bundle() {
    return b.bundle()
      // log errors if they happen
      .on('error', gutil.log.bind(gutil, 'Browserify Error'))
      .pipe(source('bundle.js'))
      .pipe(gulp.dest(paths.browserifyDest));
}