const eleventyRemarkInternal = require("@fec/eleventy-plugin-remark/src/eleventyRemark");
const collect = require("collect.js");

const defaultEleventyRemarkOptions = {
    plugins: [],
    enableRehype: true,
};

const startMarkdownCommentRegex = /\(\*(\*)+/;
const endMarkdownCommentRegex = /(\*)+\)/;

/**
 * Skip the empty lines at the beginning of the collection
 * it stop at the first non-empty line
 */
collect().macro("skipEmptyLinesAtTheBeginning", function () {
    return this.skipWhile((line) => line === "");
});

/**
 * Skip empty lines at the end of the collection
 */
collect().macro("skipEmptyLinesAtTheEnd", function () {
    return this.reverse().skipEmptyLinesAtTheBeginning().reverse();
});

collect().macro("add", function (item) {
    this.items.unshift(item);

    return this;
});

/**
 *
 * @param {collect.Collection} result - Processed lines
 * @param {collect.Collection} lines - Content to process
 */
function processFile(result, lines) {
    const line = lines.shift();

    if (line === null) {
        return result.join("\n");
    }

    const trimmedLine = line.trim();

    if (trimmedLine === "(*** hide ***)") {
        const rest = lines.skipUntil((line) => line.trim().startsWith("(**"));

        return processFile(result, rest);
    } else if (trimmedLine === "(*** show ***)") {
        // Skip this instruction line
        return processFile(result, lines);
    } else if (trimmedLine.startsWith("(**")) {
        // This is the start of an F# markdown comment
        // Strip the markdown comment start delimiter
        result.push(line.replace(startMarkdownCommentRegex, ""));

        const markdownLines = lines.takeUntil((line) => {
            return line.trimEnd().endsWith("*)");
        });

        const rest = lines.skip(markdownLines.count());

        const newResult = result.concat(markdownLines);

        const endLine = rest.first();

        // Check if the line ending the markdown comment has some content
        // If yes, capture it and add it to the result
        if (endLine) {
            newResult.push(endLine.replace(endMarkdownCommentRegex, ""));
        }

        return processFile(newResult, rest);
    } else {
        const codeLines = lines.takeUntil((line) => {
            return line.trim().startsWith("(**");
        });

        const rest = lines.skip(codeLines.count());

        // Remove non meaningful lines
        const sanetizedCodeLines = codeLines
            .skipEmptyLinesAtTheBeginning()
            .skipEmptyLinesAtTheEnd();

        let actualCode = collect();

        // If there are actual code line add them inside of a code block
        if (sanetizedCodeLines.isNotEmpty()) {
            actualCode = collect(["```fs"])
                .concat(sanetizedCodeLines)
                .concat(["```"]);
        }

        const newResult = result.concat(actualCode);

        return processFile(newResult, rest);
    }
}

function transformFsxToMarkdown(fileContent) {
    const lines = fileContent.replace(/\r\n/g, "\n").split("\n");

    // Remove empty lines at the beginning of the file
    const sanetizedLines = collect(lines).skipEmptyLinesAtTheBeginning();

    return processFile(collect(), sanetizedLines);
}

const fsharpExtension = (pluginOptions) => {
    const options = Object.assign(
        {},
        defaultEleventyRemarkOptions,
        pluginOptions
    );

    return {
        compile: async (inputContent) => {
            return async (data) => {
                const remark = eleventyRemarkInternal(options);
                const markdownContent = transformFsxToMarkdown(inputContent);
                return remark.render(markdownContent, data);
            };
        },
    };
};

module.exports = {
    initArguments: {},
    configFunction: (eleventyConfig, pluginOptions = {}) => {
        // Custom front matter delimiters to use valid F# code
        eleventyConfig.setFrontMatterParsingOptions({
            delimiters: ["(***", "***)"],
        });

        // Add F# file support to eleventy
        eleventyConfig.addTemplateFormats("fsx");
        eleventyConfig.addTemplateFormats("fs");

        // Configure how to handle fsx files
        eleventyConfig.addExtension("fsx", fsharpExtension(pluginOptions));
        eleventyConfig.addExtension("fs", fsharpExtension(pluginOptions));
    },
};
