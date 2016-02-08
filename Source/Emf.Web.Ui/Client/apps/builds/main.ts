
import $ = require("../../libs/jquery");
import _ = require("lodash");
import log = require("../../libs/log");

const list = ["a", "b", "Z!!!"];

_.forEach(list, (item: string) => document.write(item));

