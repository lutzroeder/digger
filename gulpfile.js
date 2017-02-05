var gulp = require("gulp");
var transform = require('gulp-transform');
var rename = require('gulp-rename');
var merge = require('gulp-merge');
var sourcemaps = require('gulp-sourcemaps');
var typescript = require('gulp-typescript');
var uglify = require('gulp-uglify');
var open = require('gulp-open');

gulp.task("build", [ "build:page", "build:code" ], function() {
});

gulp.task("build:code", function() {
    var sources = gulp.src([ "./src/*.ts", "./lib/*.ts" ]);
    var resources = gulp.src([ "./src/*.gif", "./src/*.wav" ])
        .pipe(transform(function(contents, file) { return "window['" + file.relative + "'] = '" + contents.toString("base64") + "';"; }))
        .pipe(rename(function(path) { path.extname += ".ts"; }));
    return merge(sources, resources)
        .pipe(sourcemaps.init())
        .pipe(typescript({ target: "ES5", out: "digger.js" }))
        .once("error", function() { this.once("finish", () => process.exit(1)) })
        .pipe(uglify())
        .pipe(sourcemaps.write("."))
        .pipe(gulp.dest("./build"))
});

gulp.task("build:page", function() {
    return gulp.src([ "./src/index.html" ])
        .pipe(gulp.dest("./build"))
});

gulp.task("default", [ "build" ], function() {
    return gulp.src("./build/index.html")
        .pipe(open());
});
