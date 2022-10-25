// @ts-check

import fs from "fs/promises";
import crypto from "crypto";
import path from "path";
import { asyncFilterCallback } from "@11ty/eleventy";

export default async function addHashFilter (
    absolutePath: string,
    callback: asyncFilterCallback
) {
    try {
        const content = await fs.readFile(path.join(".", absolutePath), {
            encoding: "utf-8",
        });

        const hash = crypto.createHash("sha256").update(content).digest("hex");

        const resultPath = `${absolutePath}?hash=${hash.slice(0, 10)}`;

        callback(null, resultPath);
    } catch (error) {
        callback(new Error(`Failed to addHash to '${absolutePath}': ${error}`));
    }
};

module.exports = addHashFilter;
