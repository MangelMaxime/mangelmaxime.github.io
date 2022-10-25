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
    // Disable use of .gitingore because some of the files are not
    // tracked in git but needs to be used by Eleventy.
    eleventyConfig.setUseGitIgnore(false);

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

    // Filters
    eleventyConfig.addNunjucksFilter("fav_icon_from_emoji", require("./_11ty/_js/filters/favIconFromEmoji"));
    eleventyConfig.addNunjucksAsyncFilter("last_modified_date", require("./_11ty/_js/filters/lastModifiedDate"));
    eleventyConfig.addNunjucksFilter("format_date", require("./_11ty/_js/filters/formatDate"));
    eleventyConfig.addNunjucksAsyncFilter("add_hash", require("./_11ty/_js/filters/addHash"));
    eleventyConfig.addNunjucksFilter("file_to_body_class", require("./_11ty/_js/filters/fileToBodyClass"));
    eleventyConfig.addNunjucksFilter("layout_to_body_class", require("./_11ty/_js/filters/layoutToBodyClass"));
    eleventyConfig.addNunjucksAsyncFilter("to_icon", require("./_11ty/_js/filters/toIcon")());

    // Shortcodes
    // eleventyConfig.addNunjucksFilter("nacara_menu", require("./_11ty/filters/menu"));
    // eleventyConfig.addNunjucksFilter("nacara_breadcrumb", require("./_11ty/filters/breadcrumb"));

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
