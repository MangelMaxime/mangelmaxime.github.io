const eleventyRemark = require("@fec/eleventy-plugin-remark");
const remarkVscode = require("gatsby-remark-vscode");
const rehypeRaw = require("rehype-raw");
const rehypeStringify = require("rehype-stringify");
const remarkToRehype = require("remark-rehype");
const PostCSSPlugin = require("eleventy-plugin-postcss");

module.exports = function (eleventyConfig) {
    // set copy asset folder to dist
    eleventyConfig.addPassthroughCopy("assets");

    // Plugins
    eleventyConfig.addPlugin(PostCSSPlugin);

    eleventyConfig.addPlugin(eleventyRemark, {
        enableRehype: false,
        plugins: [
            {
                plugin: remarkVscode.remarkPlugin,
                options: {
                    inlineCode: {
                        marker: "•"
                    },
                },
            },
            {
                plugin: remarkToRehype,
                options: {
                    allowDangerousHTML: true,
                },
            },
            rehypeRaw,
            {
                plugin: rehypeStringify,
                options: {
                    allowDangerousHTML: true,
                    closeSelfClosing: true,
                },
            },
        ],
    });

    // set input and output folder
    return {
        dir: { input: "src" },
        dataTemplateEngine: "njk",
        markdownTemplateEngine: "njk",
    };
};
