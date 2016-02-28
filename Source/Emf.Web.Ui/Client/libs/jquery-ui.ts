import $ = require("./jquery");
import { usingOverride } from "../common/variables";

var override = usingOverride(window, { $, jQuery: $ });

import "./jquery-ui/jquery-ui";
import "./jquery-ui/jquery-ui.css";

override.dispose();