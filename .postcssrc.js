const isProduction = process.env.ELEVENTY_ENV === "prod";

module.exports = {
    plugins: [
        require("autoprefixer"),
        isProduction &&
            require("cssnano")({
                preset: "default",
            }),
    ].filter(Boolean),
};
