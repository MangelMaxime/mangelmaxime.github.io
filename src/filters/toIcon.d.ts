import { asyncFilterCallback } from "@11ty/eleventy";
interface IconInformation {
    iconName: string;
    attributes: IconAttributeInformation[];
}
interface IconAttributeInformation {
    name: string;
    value: string;
}
declare type GeneratorFunction = Promise<string | Error>;
interface Options {
    [key: string]: (iconString: string) => GeneratorFunction;
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
export declare function extractIconInformation(iconString: string): IconInformation;
export declare function setIconAttributes(svgElement: any, attributes: IconAttributeInformation[]): void;
export declare function lucideGenerator(iconString: string): GeneratorFunction;
export declare function simpleIconsGenerator(iconString: string): GeneratorFunction;
export declare function assetsGenerator(iconString: string): GeneratorFunction;
/**
 *
 * @param options
 * @returns An instance of a filter able to convert a string to an icon
 */
export default function toIconFilterBuilder(options: Options): (icon: string, callback: asyncFilterCallback) => Promise<void>;
export {};
