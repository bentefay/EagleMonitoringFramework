
var _ = <_.LoDashStatic>require("lodash");

var classAttributeRegex = /class=(?:["'].*?["']|\S*)/g;
var classRegex = /\{(.*?)\}/g;

var dataBindRegex = /data-bind=(["'])(.*?)\1/g;
var stringRegex = /(["'])(.*?)\1/g;

var globalCssClasses = require('setting: css-classes');

var replaceClasses = (cssClasses, match) => {
    var newClassAttribute = match.replace(classRegex, (match, capture1) => {
        var oldClass = <string>capture1;
        var newClass: string;
        if (oldClass in cssClasses) {
            newClass = cssClasses[oldClass];
        } else if (oldClass in globalCssClasses) {
            newClass = globalCssClasses[oldClass];
        } else {
            throw new Error("Could not find css class: '" + oldClass + "'");
        }
        return newClass;
    });
    return newClassAttribute;
};

var applyCss = (html: string, cssClasses: { [id: string]: string; }) => {

    var replaceClassesClosed = match => replaceClasses(cssClasses, match);

    var newHtml = html.replace(classAttributeRegex, replaceClassesClosed);

    newHtml = newHtml.replace(dataBindRegex, (match, quotes, dataBindContent) => {

        var newDataBindContent = dataBindContent.replace(stringRegex, (match, quotes, stringContent) => {
            var replacedClasses = replaceClassesClosed(stringContent);
            replacedClasses = quotes + replacedClasses + quotes;
            // console.log("apply-css: " + match + " => " + replacedClasses);
            return replacedClasses;
        });

        return "data-bind=" + quotes + newDataBindContent + quotes;
    });

    return newHtml;
};

export = applyCss