const lucideIcons = require("lucide-static");
const { createSVGWindow } = require("svgdom");
const window = createSVGWindow();
const document = window.document;
const { SVG, registerWindow } = require("@svgdotjs/svg.js");
const simpleIcons = require("simple-icons/icons");
const fs = require("fs/promises");
const path = require("path");

/** @typedef {import('@svgdotjs/svg.js').SVGTypeMapping<T>} SVGTypeMapping */

/**
 * @typedef IconInformation
 * @type {Object}
 * @property {string} iconName The name of the icon
 * @property {IconAttributeInformation []} attributes
 */

/**
 * @typedef IconAttributeInformation
 * @type {Object}
 * @property {string} name The name of the attribute
 * @property {string} value The value of the attribute
 */

// register window and document
registerWindow(window, document);

// TODO: Add a cache to avoid importing/computing the same icon multiple times
// We can use the iconString as a key and implements the cache directly in the
// main toIconFilterBuilder function.
// This avoid the need to implement a cache for each generator
// A cache should improve the performance especially if the footer is using a lot of them
// because the footer is rendered on every page

/**
 *
 * @param {string} text The text to camelize
 * @returns The text camelized
 */
function camelCase(text) {
    return text.replace(/-([a-z])/g, function (g) {
        return g[1].toUpperCase();
    });
}

async function fileExists(path) {
    try {
        await fs.access(path);
        return true;
    } catch {
        return false;
    }
}

/**
 * Extract the information from an iconString
 *
 * Format of the iconString : <icon-name>:<attribute-name>=<attribute-value>;<attribute-name>=<attribute-value>
 *
 * Example: mail:width=20;height=20
 *
 * @param {string} iconString
 * @return {IconInformation}
 */
function extractIconInformation (iconString) {
    // If the icon string doesn't have a ';' it means that there is no options
    // we can return the icon string as is
    if (iconString.indexOf(";") === -1) {
        return {
            iconName: iconString,
            attributes: [],
        };
    } else {
        const [iconName, ...options] = iconString.split(";");
        const attributes = [];

        options.forEach((option) => {
            const [name, value] = option.split("=");
            attributes.push({
                name: name,
                value: value
            });
        });

        return {
            iconName: iconName,
            attributes: attributes,
        };
    }
}

/**
 *
 * @param {SVGTypeMapping} svgElement
 * @param {IconAttributeInformation []} attributes
 */
function setIconAttributes(svgElement, attributes) {
    for (const attribute of attributes) {
        svgElement.attr(attribute.name, attribute.value);
    }
}

const lucideGenerator = function (iconString) {
    const { iconName, attributes } = extractIconInformation(iconString);
    const lucideIcon = lucideIcons[camelCase(iconName)];

    if (lucideIcon) {
        const lucideIconSvg = SVG(lucideIcon);
        setIconAttributes(lucideIconSvg, attributes);

        return lucideIconSvg.svg();
    } else {
        return new Error(`Icon ${iconName} not found in Lucide`);
    }
};

const simpleIconsGenerator = function (iconString) {
    const { iconName, attributes } = extractIconInformation(iconString);
    // Get all properties from the icon
    let simpleIcon = undefined;

    for (const [_, value] of Object.entries(simpleIcons)) {
        if (value.slug === iconName) {
            simpleIcon = value;
            break;
        }
    }

    if (simpleIcon) {
        const simpleIconSvg = SVG(simpleIcon.svg);
        // Set some default attributes which seems to make sense for most icons
        simpleIconSvg.attr("fill", "currentColor");

        setIconAttributes(simpleIconSvg, attributes);

        return simpleIconSvg.svg();
    } else {
        return new Error(`Icon ${iconName} not found in simple-icons`);
    }
};

const assetsGenerator = async function (iconString) {
    const { iconName, attributes } = extractIconInformation(iconString);
    const iconPath = path.join(process.cwd(), "assets", "icons", `${iconName}.svg`);

    if (fileExists(iconPath)) {
        const fileContent = await fs.readFile(iconPath);
        const iconSvg = SVG(fileContent.toString());

        // Set some default attributes which seems to make sense for most icons
        iconSvg.attr("fill", "currentColor");

        setIconAttributes(iconSvg, attributes);

        return iconSvg.svg();
    } else {
        return new Error(`Icon ${iconName} not found in 'assets/icons' folder`);
    }
};

const defaultIconOptions = {
    lucide: lucideGenerator,
    simpleIcons: simpleIconsGenerator,
    assets: assetsGenerator,
};

/**
 *
 * @param {Object} options
 * @returns An instance of a filter able to convert a string to an icon
 */
module.exports = function toIconFilterBuilder(options) {
    const iconOptions = Object.assign({}, defaultIconOptions, options);

    /**
     * @param {string} icon
     * @param {any} callback
     */
    return async function (icon, callback) {
        if (icon.indexOf(":") === -1) {
            callback(
                new Error(
                    `Failed to generate the icon. An icon should follow the format '<provider>:<icon-name|options>'`
                )
            );
        } else {
            const [provider, iconName] = icon.split(":");

            if (provider === "") {
                callback(
                    new Error(
                        `Failed to generate the icon. The provider is missing.`
                    )
                );
            } else if (iconName === "") {
                callback(
                    new Error(
                        `Failed to generate the icon. The icon name is missing.`
                    )
                );
            } else {
                const iconGenerator = iconOptions[provider];

                if (iconGenerator) {
                    const generatorResult = await iconGenerator(iconName);

                    if (generatorResult instanceof Error) {
                        callback(generatorResult);
                    } else {
                        callback(null, generatorResult);
                    }
                } else {
                    callback(
                        new Error(
                            `Failed to generate the icon. The provider '${provider}' is not supported.`
                        )
                    );
                }
            }
        }
    };
};
