"use strict";
var __createBinding = (this && this.__createBinding) || (Object.create ? (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    var desc = Object.getOwnPropertyDescriptor(m, k);
    if (!desc || ("get" in desc ? !m.__esModule : desc.writable || desc.configurable)) {
      desc = { enumerable: true, get: function() { return m[k]; } };
    }
    Object.defineProperty(o, k2, desc);
}) : (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    o[k2] = m[k];
}));
var __setModuleDefault = (this && this.__setModuleDefault) || (Object.create ? (function(o, v) {
    Object.defineProperty(o, "default", { enumerable: true, value: v });
}) : function(o, v) {
    o["default"] = v;
});
var __importStar = (this && this.__importStar) || function (mod) {
    if (mod && mod.__esModule) return mod;
    var result = {};
    if (mod != null) for (var k in mod) if (k !== "default" && Object.prototype.hasOwnProperty.call(mod, k)) __createBinding(result, mod, k);
    __setModuleDefault(result, mod);
    return result;
};
Object.defineProperty(exports, "__esModule", { value: true });
const nano_jsx_1 = __importStar(require("nano-jsx"));
function favIconFromEmojiFilter(emoji) {
    const SvgContent = () => ((0, nano_jsx_1.h)("svg", { xmlns: "http://www.w3.org/2000/svg", viewBox: "0 0 100 100" },
        (0, nano_jsx_1.h)("text", { y: ".9em", "font-size": "90" }, emoji)));
    const encodedSvg = encodeURIComponent(nano_jsx_1.default.renderSSR((0, nano_jsx_1.h)(SvgContent, null)));
    const LinkElement = () => ((0, nano_jsx_1.h)("link", { rel: "icon", href: `data:image/svg+xml,${encodedSvg}`, type: "image/svg+xml" }));
    return nano_jsx_1.default.renderSSR((0, nano_jsx_1.h)(LinkElement, null));
}
exports.default = favIconFromEmojiFilter;
module.exports = favIconFromEmojiFilter;
