"use strict";
// Idea is taken from: https://11ty.rocks/tips/layout-templating/
// Compute the class representation of a page
var __importDefault = (this && this.__importDefault) || function (mod) {
    return (mod && mod.__esModule) ? mod : { "default": mod };
};
Object.defineProperty(exports, "__esModule", { value: true });
const path_1 = __importDefault(require("path"));
function fileToBodyClassFilter(filePath) {
    // console.log(filePath);
    // Compute path without extension
    const fileDir = path_1.default.dirname(filePath);
    const fileName = path_1.default.basename(filePath, path_1.default.extname(filePath));
    const fileWithoutExt = path_1.default.join(fileDir, fileName);
    // Normalize path separators and replace them with underscores
    let bodyClass = fileWithoutExt.replaceAll(/\\/g, '/').replaceAll(/\//g, '_');
    // Remove leading underscore
    if (bodyClass.startsWith('_')) {
        bodyClass = bodyClass.substring(1);
    }
    return `page--${bodyClass}`;
}
exports.default = fileToBodyClassFilter;
;
module.exports = fileToBodyClassFilter;
