/// <reference path="../typings/all.d.ts" />

import _ = require("lodash");

function usingVariableOverrides<T>(target: T, overrides: any, action: { (target: T): void }) {

    const existingProperties = _.pick(target, _.keys(overrides));

    _.defaults(target, overrides);

    action(target);

    _.defaults(target, existingProperties);
}

export = usingVariableOverrides;