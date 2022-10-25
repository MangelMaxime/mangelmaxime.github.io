"use strict";
// Idea is taken from: https://11ty.rocks/tips/layout-templating/
// Compute the class representation of a layout
var __importDefault = (this && this.__importDefault) || function (mod) {
    return (mod && mod.__esModule) ? mod : { "default": mod };
};
Object.defineProperty(exports, "__esModule", { value: true });
const path_1 = __importDefault(require("path"));
function layoutToBodyClassFilter(layout) {
    // If layout is undefined, default to 'base'
    layout = layout || 'base';
    // Compute path without extension
    const layoutDir = path_1.default.dirname(layout);
    const layoutName = path_1.default.basename(layout, path_1.default.extname(layout));
    const layoutWithoutExt = path_1.default.join(layoutDir, layoutName);
    // Normalize path separators and replace them with underscores
    let bodyClass = layoutWithoutExt.replaceAll(/\\/g, '/').replaceAll(/\//g, '_');
    // Remove leading "layouts"
    if (bodyClass.startsWith('layouts')) {
        bodyClass = bodyClass.substring(7);
    }
    // Remove leading underscore
    if (bodyClass.startsWith('_')) {
        bodyClass = bodyClass.substring(1);
    }
    return `layout--${bodyClass}`;
}
exports.default = layoutToBodyClassFilter;
;
module.exports = layoutToBodyClassFilter;
