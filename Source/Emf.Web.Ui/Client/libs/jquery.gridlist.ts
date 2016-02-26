import $ = require("jquery");
import usingVariableOverrides = require("./using-variable-overrides");

usingVariableOverrides(window, { $, jQuery: $ }, window => {

    require("grid-list");

});

export = $;