const fs = require("fs/promises");
const crypto = require("crypto");
const path = require("path");

module.exports = async function (absolutePath, callback) {
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
