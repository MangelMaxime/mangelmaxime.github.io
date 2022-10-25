"use strict";
var __importDefault = (this && this.__importDefault) || function (mod) {
    return (mod && mod.__esModule) ? mod : { "default": mod };
};
Object.defineProperty(exports, "__esModule", { value: true });
exports.assetsGenerator = exports.simpleIconsGenerator = exports.lucideGenerator = exports.setIconAttributes = exports.extractIconInformation = void 0;
// @ts-ignore
const lucide_static_1 = __importDefault(require("lucide-static"));
// @ts-ignore
const svgdom_1 = require("svgdom");
const window = (0, svgdom_1.createSVGWindow)();
const document = window.document;
const svg_js_1 = require("@svgdotjs/svg.js");
const icons_1 = __importDefault(require("simple-icons/icons"));
const promises_1 = __importDefault(require("fs/promises"));
const path_1 = __importDefault(require("path"));
// register window and document
(0, svg_js_1.registerWindow)(window, document);
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
        await promises_1.default.access(path);
        return true;
    }
    catch {
        return false;
    }
}
/**
 * Extract the information from an iconString
 *
 * Format of the iconString : `<icon-name>:<attribute-name>=<attribute-value>;<attribute-name>=<attribute-value>`
 *
 * Example: mail:width=20;height=20
 *
 * @param iconString
 * @returns The icon information
 */
function extractIconInformation(iconString) {
    // If the icon string doesn't have a ';' it means that there is no options
    // we can return the icon string as is
    if (iconString.indexOf(";") === -1) {
        return {
            iconName: iconString,
            attributes: [],
        };
    }
    else {
        const [iconName, ...options] = iconString.split(";");
        const attributes = [];
        options.forEach((option) => {
            const [name, value] = option.split("=");
            attributes.push({
                name: name,
                value: value,
            });
        });
        return {
            iconName: iconName,
            attributes: attributes,
        };
    }
}
exports.extractIconInformation = extractIconInformation;
function setIconAttributes(svgElement, attributes) {
    for (const attribute of attributes) {
        svgElement.attr(attribute.name, attribute.value);
    }
}
exports.setIconAttributes = setIconAttributes;
async function lucideGenerator(iconString) {
    const { iconName, attributes } = extractIconInformation(iconString);
    const lucideIcon = lucide_static_1.default[camelCase(iconName)];
    if (lucideIcon) {
        const lucideIconSvg = (0, svg_js_1.SVG)(lucideIcon);
        setIconAttributes(lucideIconSvg, attributes);
        return lucideIconSvg.svg();
    }
    else {
        return new Error(`Icon ${iconName} not found in Lucide`);
    }
}
exports.lucideGenerator = lucideGenerator;
async function simpleIconsGenerator(iconString) {
    const { iconName, attributes } = extractIconInformation(iconString);
    // Get all properties from the icon
    let simpleIcon = undefined;
    for (const [_, value] of Object.entries(icons_1.default)) {
        if (value.slug === iconName) {
            simpleIcon = value;
            break;
        }
    }
    if (simpleIcon) {
        const simpleIconSvg = (0, svg_js_1.SVG)(simpleIcon.svg);
        // Set some default attributes which seems to make sense for most icons
        simpleIconSvg.attr("fill", "currentColor");
        setIconAttributes(simpleIconSvg, attributes);
        return simpleIconSvg.svg();
    }
    else {
        return new Error(`Icon ${iconName} not found in simple-icons`);
    }
}
exports.simpleIconsGenerator = simpleIconsGenerator;
async function assetsGenerator(iconString) {
    const { iconName, attributes } = extractIconInformation(iconString);
    const iconPath = path_1.default.join(process.cwd(), "assets", "icons", `${iconName}.svg`);
    if (await fileExists(iconPath)) {
        const fileContent = await promises_1.default.readFile(iconPath);
        const iconSvg = (0, svg_js_1.SVG)(fileContent.toString());
        // Set some default attributes which seems to make sense for most icons
        iconSvg.attr("fill", "currentColor");
        setIconAttributes(iconSvg, attributes);
        return iconSvg.svg();
    }
    else {
        return new Error(`Icon ${iconName} not found in 'assets/icons' folder`);
    }
}
exports.assetsGenerator = assetsGenerator;
const defaultIconOptions = {
    lucide: lucideGenerator,
    simpleIcons: simpleIconsGenerator,
    assets: assetsGenerator,
};
/**
 *
 * @param options
 * @returns An instance of a filter able to convert a string to an icon
 */
function toIconFilterBuilder(options) {
    const iconOptions = Object.assign({}, defaultIconOptions, options);
    /**
     * @param {string} icon
     * @param {any} callback
     */
    return async function (icon, callback) {
        if (icon.indexOf(":") === -1) {
            callback(new Error(`Failed to generate the icon. An icon should follow the format '<provider>:<icon-name|options>'`));
        }
        else {
            const [provider, iconName] = icon.split(":");
            if (provider === "") {
                callback(new Error(`Failed to generate the icon. The provider is missing.`));
            }
            else if (iconName === "") {
                callback(new Error(`Failed to generate the icon. The icon name is missing.`));
            }
            else {
                const iconGenerator = iconOptions[provider];
                if (iconGenerator) {
                    const generatorResult = await iconGenerator(iconName);
                    if (generatorResult instanceof Error) {
                        callback(generatorResult);
                    }
                    else {
                        callback(null, generatorResult);
                    }
                }
                else {
                    callback(new Error(`Failed to generate the icon. The provider '${provider}' is not supported.`));
                }
            }
        }
    };
}
exports.default = toIconFilterBuilder;
module.exports = toIconFilterBuilder;
