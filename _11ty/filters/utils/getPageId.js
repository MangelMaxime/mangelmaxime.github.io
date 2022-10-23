// @ts-check
const path = require("path");
const NacaraTypes = require("./../../types/nacara");

/**
 *
 * @param {string} fileStem
 * @returns {NacaraTypes.PageId} The pageId representing the provided fileStem
 */
 module.exports = function getPageId(fileStem) {
    //  Normal the path, so we can split using the path separator
    const normalizedInputPath = path.normalize(fileStem);
    // Extract all the segments of the path
    const inputPathSegments = normalizedInputPath.split(path.sep);
    // console.log("inputPathSegments:", inputPathSegments);
    // Build the section direction, which consist of the root + the first segment of the path
    const pageIdSegments = inputPathSegments.slice(2);

    return pageIdSegments.join("/");
}
