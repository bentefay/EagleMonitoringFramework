import $ = require("jquery");
import { usingOverride } from "../common/variables";
import "./jquery-ui"

var override = usingOverride(window, { $, jQuery: $ });
    
require("./gridlist/gridlist");
require("./gridlist/jquery.gridlist");

override.dispose();