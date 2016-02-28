var loaderUtils = require("loader-utils");
var path = require("path");

module.exports = function (content) {
    this.cacheable && this.cacheable();

    var header = "/* File: " + path.basename(this.resourcePath) + " */";
    return header + "\n" + content;
};