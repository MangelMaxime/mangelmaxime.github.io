// @ts-check
const EleventyTypes = require("./eleventy");

/**
 * @typedef {string} PageId
 */

/**
 * @typedef {object} MenuSection
 * @property {'section'} type
 * @property {string} label
 * @property {MenuItem []} items
 */

/**
 * @typedef {object} MenuLink
 * @property {'link'} type
 * @property {string} label
 * @property {string} href
 */

/**
 * @typedef {MenuSection|MenuLink|string} MenuItem
 */

/**
 * @typedef {MenuItem []} Menu
 */


/**
 * @typedef {object} PageData
 * @property {Menu} [menu]
 */


module.exports.Types = {};
