var gutil = require('gulp-util');
var cssp = require('cssp');
var _ = require('lodash-compat');

var selectorParser = {

    parse: function (selector) {

        // Parse selector using different parser which fully parses selectors - needs braces to make it valid css.
        var selectorAst = cssp.parse(selector + "{}");

        var parentNode = getNode(selectorAst, ["stylesheet", "ruleset", "selector"]);
        if (parentNode === null)
            return null;

        var selectorNode = getNode(parentNode, ["selector", "simpleselector"]);
        if (selectorNode === null)
            return null;

        var selectorTokens = _.rest(selectorNode); // skip the first element, which is "simpleselector"

        var wrappedTokens = _.map(selectorTokens, function(t) { return wrapToken(t); });

        return { tokens: wrappedTokens, _ast: selectorAst, _parentNode: parentNode };
    },

    stringify: function(ast) {
        var tokens = _.map(ast.tokens, function(t) { return unwrapToken(t); });

        // Update parent node with new tokens
        var selectorNode = ["simpleselector"].concat(tokens);
        ast._parentNode[1] = selectorNode;

        var modifiedSelector = cssp.translate(ast._ast);

        // Remove braces that we added
        modifiedSelector = modifiedSelector.substring(0, modifiedSelector.length - 2);

        return modifiedSelector;
    },

    createTag: function(name) {
        return this._createTag(["ident", name]);
    },

    _createTag: function(token) {
        return {
            type: "tag",
            name: function(value) {
                if (value) this._token[1] = value;
                return this._token[1];
            },
            fullName: function() { return this.name(); },
            isIdentifier: true,
            _token: token
        };
    },

    createClass: function(name) {
        return this._createClass(["clazz", ["ident", name]]);
    },

    _createClass: function(token) {
        return {
            type: "class",
            name: function (value) {
                if (value) this._token[1][1] = value;
                return this._token[1][1];
            },
            fullName: function() { return "." + this.name(); },
            isIdentifier: true,
            _token: token
        };
    },

    createId: function(name) {
        return this._createId(["shash", name]);
    },

    _createId: function(token) {
        return {
            type: "id",
            name: function (value) {
                if (value) this._token[1] = value;
                return this._token[1];
            },
            fullName: function() { return "#" + this.name(); },
            isIdentifier: true,
            _token: token
        };
    },

    createIdentifier: function(fullName) {
        if (fullName[0] === ".")
            return this.createClass(fullName.substring(1));
        if (fullName[0] === "#")
            return this.createId(fullName.sustring(1));
        return this.createTag(fullName);
    },

    createWhitespace: function(value) {
        return this._createWhitespace(['s', value]);
    },

    _createWhitespace: function(token) {
        return {
            type: "whitespace",
            isWhitespace: true,
            value: function (value) {
                if (value) this._token[1] = value;
                return this._token[1];
            },
            _token: token
        }
    },

    _createUnknown: function(token) {
        return {
            type: "unknown",
            _token: token
        };
    }
};

function wrapToken(token) {

    // selectorTokens of form: [<token0>, <token1>, ..., <tokenN>]
    // where <tokenN> of form:
    // type = css example = node structure
    // whitespace = " \t " = ["s"," \t "]
    // tag = "div" or "*" = ["ident","div"]
    // class = ".classX" = ["clazz",["ident", "classX"]]
    // id = "#idX" = ["shash","idX"]
    // combinator = "+" or ">" or "~" = ["combinator","+"]
    // psuedo class = ":active" = ["pseudoc",["ident","active"]]]
    // psuedo element = "::after" = ["pseudoe",["ident","after"]]]
    // function = ":not(p)" = ["pseudoc",["funktion",["ident","not"],["functionBody",["ident","p"]]]]
    // attribute = "[target=_blank]" = ["attrib",["ident","target"],["attrselector","="],["ident","_blank"]]

    // Class
    if (token[0] === "clazz" && token[1][0] === "ident") {
        return selectorParser._createClass(token);
    }

    // Tag
    if (token[0] === "ident") {
        return selectorParser._createTag(token);
    }

    // Id
    if (token[0] === "shash") {
        return selectorParser._createId(token);
    }

    // Whitespace
    if (token[0] === "s") {
        return selectorParser._createWhitespace(token);
    }

    return selectorParser._createUnknown(token);
}

function unwrapToken(token) {
    return token._token;
}

function getNode(selectorAst, expectedTypes) {

    var node = selectorAst;
    var finalType = expectedTypes.pop();

    for (var i = 0; i < expectedTypes.length; i++) {

        var expectedType = expectedTypes[i];

        if (!node || !node.length || node.length <= 1 || node[0] !== expectedType) {
            gutil.log("Expected " + expectedType + " but found " + JSON.stringify(node));
            return null;
        }

        node = node[1];
    };

    if (!node || !node.length || node.length === 0 || node[0] !== finalType) {
        gutil.log("Expected " + finalType + " but found " + JSON.stringify(node));
        return null;
    }

    return node;
}

module.exports = selectorParser;