"use strict";
// @ts-check
var __importDefault = (this && this.__importDefault) || function (mod) {
    return (mod && mod.__esModule) ? mod : { "default": mod };
};
Object.defineProperty(exports, "__esModule", { value: true });
const promises_1 = __importDefault(require("fs/promises"));
const crypto_1 = __importDefault(require("crypto"));
const path_1 = __importDefault(require("path"));
async function addHashFilter(absolutePath, callback) {
    try {
        const content = await promises_1.default.readFile(path_1.default.join(".", absolutePath), {
            encoding: "utf-8",
        });
        const hash = crypto_1.default.createHash("sha256").update(content).digest("hex");
        const resultPath = `${absolutePath}?hash=${hash.slice(0, 10)}`;
        callback(null, resultPath);
    }
    catch (error) {
        callback(new Error(`Failed to addHash to '${absolutePath}': ${error}`));
    }
}
exports.default = addHashFilter;
;
module.exports = addHashFilter;
