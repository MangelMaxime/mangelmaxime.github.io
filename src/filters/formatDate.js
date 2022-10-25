"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const luxon_1 = require("luxon");
function formatDateFilter(date) {
    return luxon_1.DateTime.fromJSDate(date, { zone: "utc" }).toFormat("yyyy-LL-dd HH:mm:ss");
}
exports.default = formatDateFilter;
module.exports = formatDateFilter;
