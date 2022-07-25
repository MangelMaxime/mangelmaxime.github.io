const minify = require("html-minifier").minify;

module.exports = (content, outputPath) => {
    if (outputPath && outputPath.endsWith(".html")) {
        return minify(content, {
            removeAttributeQuotes: true,
            collapseBooleanAttributes: true,
            collapseWhitespace: true,
            removeComments: true,
            sortClassName: true,
            sortAttributes: true,
            html5: true,
            decodeEntities: true,
            removeOptionalTags: true,
        });
    }

    return content;
};
