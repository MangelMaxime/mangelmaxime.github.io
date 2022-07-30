const purgecss = require("@fullhuman/postcss-purgecss");
const csso = require("postcss-csso");
const postcssEnvPresent = require("postcss-preset-env");
const postcssSass = require('@csstools/postcss-sass');


// Get env variables
const isProduction = true//process.env.FORNAX_ENV === "prod";

module.exports = {
    plugins: [
        postcssSass(),
        postcssEnvPresent(),
        isProduction && purgecss({
            content: ["./_public/**/*.html"],
            variables: true,
        }),
        isProduction && csso(),
    ].filter(Boolean),
    syntax: require('postcss-scss')
};
