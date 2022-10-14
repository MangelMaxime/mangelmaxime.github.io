const eleventyFsharpLiterate = require("./_11ty/plugins/fsharp-literate");
const eleventyRemark = require("@fec/eleventy-plugin-remark");
const remarkVscode = require("gatsby-remark-vscode");
const rehypeRaw = require("rehype-raw");
const rehypeStringify = require("rehype-stringify");
const remarkToRehype = require("remark-rehype");
const eleventySass = require("eleventy-sass");

const remarkOptions = {
    enableRehype: false,
    plugins: [
        {
            plugin: remarkVscode.remarkPlugin,
            options: {
                inlineCode: {
                    marker: "•",
                },
            },
        },
        {
            plugin: remarkToRehype,
            options: {
                allowDangerousHtml: true,
            },
        },
        rehypeRaw,
        {
            plugin: rehypeStringify,
            options: {
                allowDangerousHtml: true,
                closeSelfClosing: true,
            },
        },
    ],
};

module.exports = function (eleventyConfig) {
    // set copy asset folder to dist
    eleventyConfig.addPassthroughCopy("assets");

    // Plugins
    // eleventyConfig.addPlugin(postCSSPlugin);
    // eleventyConfig.addPlugin(inclusiveLanguagePlugin);

    // if (process.env.ELEVENTY_ENV === "prod") {
    //     eleventyConfig.addPlugin(purgeCssPlugin, {
    //         config: "./.purgecssrc.js",
    //     });

    //     eleventyConfig.addTransform("htmlmin", require("./_11ty/htmlminify.js"));
    // }

    eleventyConfig.addPlugin(eleventySass);

    eleventyConfig.addPlugin(eleventyRemark, remarkOptions);
    eleventyConfig.addPlugin(eleventyFsharpLiterate({
        eleventyRemarkOptions: remarkOptions
    }));
    // console.log(eleventyConfig);

    // Filters
    eleventyConfig.addNunjucksFilter("favIconFromEmoji", require("./_11ty/filters/favIconFromEmoji"));
    eleventyConfig.addNunjucksAsyncFilter("lastModifiedDate", require("./_11ty/filters/lastModifiedDate"));
    eleventyConfig.addNunjucksFilter("formatDate", require("./_11ty/filters/formatDate"));
    eleventyConfig.addNunjucksAsyncFilter("addHash", require("./_11ty/filters/addHash"));
    eleventyConfig.addNunjucksFilter("file_to_body_class", require("./_11ty/filters/file_to_body_class"));
    eleventyConfig.addNunjucksFilter("layout_to_body_class", require("./_11ty/filters/layout_to_body_class"));
    // console.log(eleventyConfig);


    // eleventyConfig.addPlugin(eleventyRemark, remarkOptions);
    // eleventyConfig.addPlugin(eleventyFsharpLiterate, remarkOptions);

    // set input and output folder
    return {
        dir: {
            input: ".",
            includes: "_includes",
            data: "_data",
            output: "_site",
        },
        dataTemplateEngine: "njk",
        htmlTemplateEngine: "njk",
        markdownTemplateEngine: "njk",
    };
};
