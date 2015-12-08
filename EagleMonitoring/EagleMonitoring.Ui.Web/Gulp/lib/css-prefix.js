var cssParser = require('css');
var traverse = require('traverse');
var _ = require('lodash-compat');
var gutil = require('gulp-util');
var selectorParser = require('./css-selector-parser');

module.exports = function (opts, src) {

    _.defaults(opts, {
        insertParent: null,             // Wrap all selectors inside this identifier
        identifiersOutsideParent: [],   // Do not wrap these identifiers in the parent identifier
        identifiersToDelete: [],        // Remove these identifiers
        insertClassPrefix: null,        // Prefix all classes with this string
        insertIdPrefix: null            // Prefix all ids with this string
    });

    var parentToken = null;
    if (opts.insertParent) {
        parentToken = selectorParser.createIdentifier(opts.insertParent);
    }

    var identifiersOutsideParentLookup = {};
    _.forEach(opts.identifiersOutsideParent, function(identifier) {
        identifiersOutsideParentLookup[identifier] = true;
        return true;
    });

    var identifiersToDeleteLookup = {};
    _.forEach(opts.identifiersToDelete, function(identifier) {
        identifiersToDeleteLookup[identifier] = true;
        return true;
    });

    var whitespaceToken = selectorParser.createWhitespace(" ");

    function modifySelectorTokens(selectorTokens) {

        // Delete tags
        if (opts.identifiersToDelete.length > 0) {
            selectorTokens = _.filter(selectorTokens, function(token) {
                return !(token.isIdentifier && identifiersToDeleteLookup[token.fullName()]);
            });
        }

        // Insert parent
        if (parentToken) {
            var indexToInsertParent = 0;
            _.forEach(selectorTokens, function (token, index) {
                if (token.isIdentifier && identifiersOutsideParentLookup[token.fullName()]) {
                    indexToInsertParent = index + 1;
                }
                return true;
            });

            // Unless there are no selectors, do not insert parent if it is the last token, as this selector should lie entirely outside the parent identifier
            if (indexToInsertParent === 0 || indexToInsertParent !== selectorTokens.length)
                selectorTokens.splice(indexToInsertParent, 0, whitespaceToken, parentToken, whitespaceToken);
        }

        // Prefix
        _.forEach(selectorTokens, function (token) {
            if (token === parentToken)
                return true;

            if (opts.insertClassPrefix && token.type === "class") {
                token.name(opts.insertClassPrefix + token.name());

            } else if (opts.insertIdPrefix && token.type === "id") {
                token.name(opts.insertIdPrefix + token.name());
            }

            return true;
        });

        // Remove starting and ending whitespace and set all whitespace to " "
        selectorTokens = _(selectorTokens)
            .dropWhile(function (token) { return token.isWhitespace; })
            .dropRightWhile(function (token) { return token.isWhitespace; })
            .map(function (token) {
                if (token.isWhitespace)
                    token.value(" ");
                return token;
            })
            .value();

        // Strip repeated whitespace
        var previousIsWhitespace = true;
        selectorTokens = _.filter(selectorTokens, function (token, index) {

            var exclude = token.isWhitespace && previousIsWhitespace;
            previousIsWhitespace = token.isWhitespace;
            return !exclude;
        });

        return selectorTokens;
    }


    // This tool will visualize the ast for you: http://iamdustan.com/reworkcss_ast_explorer/
    var ast = cssParser.parse(src);

    traverse(ast).forEach(function () {

        if (!isSelector(this))
            return;

        var selector = this.node;

        var selectorAst = selectorParser.parse(selector);
        if (!selectorAst)
            return;

        selectorAst.tokens = modifySelectorTokens(selectorAst.tokens);

        var modifiedSelector = selectorParser.stringify(selectorAst);

        this.update(modifiedSelector);
    });

    var result = cssParser.stringify(ast);

    return result;
};

function isSelector(astNode) {
    return astNode.parent && astNode.parent.key === "selectors";
}