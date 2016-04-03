/// <reference path="./../typings/all.d.ts"/>

import _ = require("lodash");

export type GroupedTreeNode<T> = { leaves: T[], groups: TreeNodeGroups<T> };
export type TreeNodeGroups<T> = { [key: string]: GroupedTreeNode<T> };

export function recursiveGroupBy<T>(values: T[], keySelector: (value: T) => string[]): TreeNodeGroups <T> {
    const valueAndKeysCollection = _.map(values, value => { return { value, keys: keySelector(value) }; });

    return recursiveGroupByHelper(valueAndKeysCollection, 0);
}

function recursiveGroupByHelper<T>(valueAndKeysCollection: { value: T, keys: string[] }[], keyIndex: number): TreeNodeGroups<T> {

    const valueAndKeysByKeyN = _.groupBy(valueAndKeysCollection, valueAndKeys => valueAndKeys.keys[keyIndex]);

    return _.mapValues(valueAndKeysByKeyN, valueAndKeysCollection => {

        const [leaves, notLeaves] = _.partition(valueAndKeysCollection, valueAndKeys => keyIndex === valueAndKeys.keys.length - 1);

        return { leaves: _.map(leaves, l => l.value), groups: recursiveGroupByHelper(notLeaves, keyIndex + 1) }; 
    });
}