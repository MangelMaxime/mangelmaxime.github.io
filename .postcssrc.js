// const purgecss = require("@fullhuman/postcss-purgecss");
// const csso = require("postcss-csso");
// const postcssEnvPresent = require("postcss-preset-env");
// const postcssSass = require('@csstools/postcss-sass');

// // Get env variables
// const isProduction = true//process.env.FORNAX_ENV === "prod";

// module.exports = {
//     plugins: [
//         postcssSass(),
//         postcssEnvPresent(),
//         isProduction && purgecss({
//             content: ["./public/**/*.html"],
//             variables: true,
//         }),
//         isProduction && csso(),
//     ].filter(Boolean),
//     syntax: require('postcss-scss')
// };

const purgecss = require("@fullhuman/postcss-purgecss")({
    content: ["./hugo_stats.json"],
    defaultExtractor: (content) => {
        let els = JSON.parse(content).htmlElements;
        return els.tags.concat(els.classes, els.ids);
    },
});

module.exports = {
    plugins: [
        ...(process.env.HUGO_ENVIRONMENT === "production" ? [purgecss] : []),
    ],
};
