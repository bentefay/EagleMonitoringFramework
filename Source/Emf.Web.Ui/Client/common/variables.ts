/// <reference path="../typings/all.d.ts" />

import _ = require("lodash");
import { Disposable, IDisposable } from "./disposable";

export function override<T>(target: T, overrides: any, action: { (target: T): void }) {

    const existingProperties = _.pick(target, _.keys(overrides));

    _.defaults(target, overrides);

    action(target);

    _.defaults(target, existingProperties);
}

export function usingOverride<T>(target: T, overrides: any): IDisposable {

    const existingProperties = _.pick(target, _.keys(overrides));

    _.defaults(target, overrides);

    return new Disposable(() => _.defaults(target, existingProperties));
}