const { defineConfig } = require("pollen-css/utils");

// Custom configuration for Pollen
// Another interesting project to serve as reference is
// https://github.com/argyleink/open-props/

module.exports = defineConfig((pollen) => ({
    modules: {
        elevation: false,
        grid: false,

        /**
         * Shadow
         * Applied as box-shadow
         */
        shadow: {
            1: "0 1px 2px 0 rgba(0, 0, 0, 0.05)",
            2: "0 1px 3px 0 rgba(0, 0, 0, 0.1), 0 1px 2px 0 rgba(0, 0, 0, 0.06)",
            3: "0 4px 6px -2px rgba(0, 0, 0, 0.1), 0 2px 4px -2px rgba(0, 0, 0, 0.06)",
            4: "0 12px 16px -4px rgba(0, 0, 0, 0.1), 0 4px 6px -2px rgba(0, 0, 0, 0.05)",
            5: "0 20px 24px -4px rgba(0, 0, 0, 0.1), 0 8px 8px -4px rgba(0, 0, 0, 0.04)",
            6: "0 24px 48px -12px rgba(0, 0, 0, 0.25)",
            7: "0 32px 64px -12px rgba(0, 0, 0, 0.2)",
        }
    },
    output: "./style/pollen.scss"
}));
