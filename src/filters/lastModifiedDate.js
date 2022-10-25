"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const util_1 = require("util");
const execFile = (0, util_1.promisify)(require("child_process").execFile);
// Cache the last modified date of a file
// because invoking git log is expensive
const lastModifiedDateCache = new Map();
async function lastModifiedDateFromGit(fileName) {
    try {
        const { stdout } = await execFile("git", [
            "--no-pager",
            "log",
            "-1",
            "--format=%cd",
            fileName,
        ]);
        if (stdout) {
            return new Date(stdout);
        }
        return new Date();
    }
    catch (e) {
        console.error(e.message);
        return new Date();
    }
}
async function lastModifiedDateFilter(fileName, callback) {
    const cachedValue = lastModifiedDateCache.get(fileName);
    if (cachedValue) {
        callback(null, cachedValue);
    }
    else {
        lastModifiedDateFromGit(fileName).then((date) => {
            lastModifiedDateCache.set(fileName, date);
            callback(null, date);
        });
    }
}
exports.default = lastModifiedDateFilter;
;
module.exports = lastModifiedDateFilter;
