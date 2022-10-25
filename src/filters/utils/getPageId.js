"use strict";
var __importDefault = (this && this.__importDefault) || function (mod) {
    return (mod && mod.__esModule) ? mod : { "default": mod };
};
Object.defineProperty(exports, "__esModule", { value: true });
const path_1 = __importDefault(require("path"));
// import NacaraTypes from "./../../types/nacara";
function getPageId(fileStem) {
    //  Normal the path, so we can split using the path separator
    const normalizedInputPath = path_1.default.normalize(fileStem);
    // Extract all the segments of the path
    const inputPathSegments = normalizedInputPath.split(path_1.default.sep);
    // console.log("inputPathSegments:", inputPathSegments);
    // Build the section direction, which consist of the root + the first segment of the path
    const pageIdSegments = inputPathSegments.slice(2);
    return pageIdSegments.join("/");
}
exports.default = getPageId;
;
