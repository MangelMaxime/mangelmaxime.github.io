import { promisify } from "util";
const execFile = promisify(require("child_process").execFile);
import { asyncFilterCallback } from "@11ty/eleventy";

// Cache the last modified date of a file
// because invoking git log is expensive
const lastModifiedDateCache = new Map();

async function lastModifiedDateFromGit(fileName: string) {
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
    } catch (e: any) {
        console.error(e.message);
        return new Date();
    }
}

export default async function lastModifiedDateFilter (
    fileName: string,
    callback: asyncFilterCallback
) {
    const cachedValue = lastModifiedDateCache.get(fileName);

    if (cachedValue) {
        callback(null, cachedValue);
    } else {
        lastModifiedDateFromGit(fileName).then((date) => {
            lastModifiedDateCache.set(fileName, date);
            callback(null, date);
        });
    }
};

module.exports = lastModifiedDateFilter;
