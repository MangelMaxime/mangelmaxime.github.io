const cssnano = require("cssnano");
const postcssPresetEnv = require("postcss-preset-env");
const postcssReporter = require("postcss-reporter");
const stylelint = require("stylelint");
const postcssImport = require("postcss-import");

const isProduction = process.env.ELEVENTY_ENV === "prod";

module.exports = {
    plugins: [
        postcssImport,
        postcssPresetEnv({
            features: {
                "nesting-rules": true,
            },
        }),
        isProduction &&
            cssnano({
                preset: "default",
            }),
        stylelint,
        postcssReporter({
            throwError: isProduction,
            clearReportedMessages: true,
        }),
    ].filter(Boolean),
};
