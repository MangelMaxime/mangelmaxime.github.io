// Code from this file has been based on
// https://github.com/saneef/eleventy-plugin-asciidoc/blob/da3ba916808fdb87e97f25c4451f3a78f55a5769/lib/eleventy-asciidoc.js

// @ts-check

const debug = require("debug")("eleventy-plugin-fsharp-literate");
const fs = require("fs");
const matter = require("gray-matter");
const NunjucksLib = require("nunjucks");
var MarkdownIt = require("markdown-it");
const NunjucksEngine = require("@11ty/eleventy/src/Engines/Nunjucks");
const eleventyRemarkInternal = require("@fec/eleventy-plugin-remark/src/eleventyRemark");

const defaultEleventyRemarkOptions = {
    plugins: [],
    enableRehype: true,
};

/** @typedef {import('gray-matter').GrayMatterFile}  GrayMatterFile */

const startMarkdownCommentRegex = /\(\*(\*)+/;
const endMarkdownCommentRegex = /(\*)+\)/;

/**
 * Remove elements that match the predicate from the beginning of the array
 *
 * @template T
 * @param {T[]} array The input array
 * @param {((arg0: T) => boolean)} predicate A function that evaluates an element of the array to a boolean value
 * @returns {T[]} A new array with the elements that match the predicate removed from the beginning
 */
const skipWhile = (array, predicate) => {
    let i = 0;
    while (i < array.length && predicate(array[i])) {
        i++;
    }
    return array.slice(i);
};

/**
 * Remove elements that match the predicate from the end of the array
 *
 * @template T
 * @param {T[]} array The input array
 * @param {((arg0: T) => boolean)} predicate A function that evaluates an element of the array to a boolean value
 * @returns {T[]} The elements that match the predicate
 */
const skipWhileFromEnd = (array, predicate) => {
    let i = array.length - 1;
    while (i >= 0 && predicate(array[i])) {
        i--;
    }
    return array.slice(0, i + 1);
};

/**
 * Skips items until the predicate is true and then returns the rest of the array
 *
 * @template T
 * @param {T []} array The input array
 * @param {(arg0: T) => boolean} predicate A function that evaluates an element of the array to a boolean value
 * @returns {T []} The rest of the array
 */
const skipUntil = (array, predicate) => {
    let i = 0;
    while (i < array.length && !predicate(array[i])) {
        i++;
    }

    return array.slice(i);
};

/**
 * Returns elements from the beginning of the array until the predicate is true
 *
 * @template T
 * @param {T []} array The input array
 * @param {(arg0: T) => boolean} predicate A function that evaluates an element of the array to a boolean value
 * @returns {T []} The rest of the array
 */
const takeUntil = (array, predicate) => {
    let i = 0;
    while (i < array.length && !predicate(array[i])) {
        i++;
    }

    return array.slice(0, i);
};

/**
 * Transforms a file from F# literate to markdown
 *
 * @param {string []} accumulator Accumulate the transformed lines
 * @param {string []} lines Lines to process
 * @returns {string} The markdown text resulting from the transformation
 */
function processFile(accumulator, lines) {
    const line = lines[0];

    debug("ProcessFile - accumulator:", accumulator);
    debug("ProcessFile - lines:", lines);

    // If there are no more lines, return the result of the transformation
    if (line === undefined) {
        return accumulator.join("\n");
    }

    const trimmedLine = line.trim();

    if (trimmedLine === "(*** hide ***)") {
        debug("Capture an hide block");

        // Eat the first line because it is the instruction line
        lines.shift();

        // Skip the lines until we find a new literate comment
        const rest = skipUntil(lines, (line) =>
            line.trim().startsWith("(**")
        );

        debug(`Rest of the lines:`, rest);

        return processFile(accumulator, rest);
    } else if (trimmedLine === "(*** show ***)") {
        debug("Show instruction encountered");
        // Eat the first line because it is the instruction line
        lines.shift();
        return processFile(accumulator, lines);
    } else if (trimmedLine.startsWith("(**")) {
        // Eat the first line because it is the literate comment
        lines.shift();

        // 1. Store the start of the F# markdown comment as it can contain some text
        accumulator.push(line.replace(startMarkdownCommentRegex, ""));

        // 2. Take the lines until we find the end of the markdown comment
        const markdownLines = takeUntil(lines, (line) =>
            line.trimEnd().endsWith("*)")
        );

        const rest = lines.slice(markdownLines.length);

        const newAccumulator = accumulator.concat(markdownLines);

        const endLine = rest[0];

        // Check if the line ending the markdown comment has some content
        // If yes, capture it and add it to the result
        if (endLine) {
            newAccumulator.push(endLine.replace(endMarkdownCommentRegex, ""));
        }

        // Skip the last line because it is the end of the markdown comment
        // and we don't need it anymore
        let restWithoutEndLine = rest.slice(1);

        return processFile(newAccumulator, restWithoutEndLine);
    } else {
        debug("Start capturing a code block");
        const codeLines = takeUntil(lines, (line) =>
            line.trim().startsWith("(**")
        );

        debug(`Captured ${codeLines.length} lines of code:`, codeLines);

        const rest = lines.slice(codeLines.length);

        debug(`Rest of the lines:`, rest);

        // Remove non meaningful lines
        let sanetizedCodeLines = skipWhile(codeLines, (line) => line === "");
        sanetizedCodeLines = skipWhileFromEnd(
            sanetizedCodeLines,
            (line) => line === ""
        );

        debug(`Sanetized code lines:`, sanetizedCodeLines);

        let actualCode = [];

        // If there are actual code line add them inside of a code block
        if (sanetizedCodeLines.length > 0) {
            actualCode = ["```fs"].concat(sanetizedCodeLines).concat(["```"]);
        }

        const newAccumulator = accumulator.concat(actualCode);

        debug(`New accumulator state:`, newAccumulator);

        return processFile(newAccumulator, rest);
    }
}

const transformFsxToMarkdown = (fileContent) => {
    const lines = fileContent.replace(/\r\n/g, "\n").split("\n");

    // Remove empty lines at the beginning of the file
    const sanetizedLines = skipWhile(lines, (line) => line === "");

    return processFile([], sanetizedLines);
};

/**
 * Reads a file synchronously.
 *
 * @param      {string}  inputPath  The input path
 * @return     {GrayMatterFile}  { description_of_the_return_value }
 */
const readFileSync = (inputPath) => {
    return matter(fs.readFileSync(inputPath, "utf8"), {
        delimiters: ["(***", "***)"],
    });
};

/**
 * Gets front-matter data from the file synchronously
 *
 * @param      {string}  inputPath  The input path
 * @return     {{ [key: string]: any }}  The data.
 */
const getData = (inputPath) => {
    const { data } = readFileSync(inputPath);

    return {
        // fsharpAttributes: attributes,
        ...data,
    };
};

let nunjucksEngineCache = null;

/**
 *
 * Return the instance of the Nunjucks engine if it exists, otherwise create it and return it.
 *
 * @param {object} eleventyConfig
 * @returns
 */
function getNunjucksEngine (eleventyConfig) {
    // Setup the nunjucks used by eleventy, so we can call it ourself to process the content
    // This is need otherwise, shortcodes or filters are not processed correctly

    if (!nunjucksEngineCache) {
        nunjucksEngineCache = new NunjucksEngine(
            "njk",
            {
                input: eleventyConfig.dir.input,
                includes: eleventyConfig.dir.includes,
            },
            eleventyConfig
        );
    }

    return nunjucksEngineCache;
};

/**
 * @typedef {object} FsharpLiteratePluginOptions
 * @property {object} eleventyRemarkOptions
 */

/**
 *
 * @param {object} eleventyConfig
 * @param {FsharpLiteratePluginOptions} pluginOptions
 * @returns
 */
function eleventyFsharpLiterate(eleventyConfig, pluginOptions) {
    /**
     *
     * @param {string | undefined | ((arg0: any) => any)} inputContent The content of the file
     * @param {string} inputPath The path of the file
     */
    const compile = async (inputContent, inputPath) => {
        const remarkOptions = Object.assign(
            {},
            defaultEleventyRemarkOptions,
            pluginOptions.eleventyRemarkOptions
        );

        const remark = eleventyRemarkInternal(remarkOptions);

        /**
         * @param {object} data The data object coming from the data cascade (front-matter, global data, etc.)
         */
        return async (data) => {
            if (inputContent) {
                // So if str has a value, it's a permalink (which can be a string or a function)
                debug(`Permalink: ${inputContent}`);
                return typeof inputContent === "function"
                    ? inputContent(data)
                    : NunjucksLib.renderString(inputContent, data);
            }

            debug(`Reading ${inputPath}`);
            const { content } = readFileSync(inputPath);

            const nunjucksEngine = getNunjucksEngine(eleventyConfig);

            if (content) {
                debug(`Converting fsx:\n ${content}`);

                const markdownContent = transformFsxToMarkdown(content);

                debug(`Markdown content:\n ${markdownContent}`);

                const markdownText = await remark.render(markdownContent, data);
                const nunjucksCompileFunc = await nunjucksEngine.compile(
                    markdownText,
                    inputPath
                );
                return await nunjucksCompileFunc(data);
            }
        };
    };

    return {
        read: false,
        getData,
        compile,
    };
}

/**
 *
 * @param {FsharpLiteratePluginOptions} pluginOptions
 */
function configFunction(pluginOptions) {
    /**
     * @param {object} eleventyConfig
     */
    return (eleventyConfig) => {
        eleventyConfig.addTemplateFormats("fsx");
        eleventyConfig.addExtension("fsx", eleventyFsharpLiterate(eleventyConfig, pluginOptions));
    }
}

module.exports = configFunction;
