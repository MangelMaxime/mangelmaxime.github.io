"use strict";
// @ts-check
var __importDefault = (this && this.__importDefault) || function (mod) {
    return (mod && mod.__esModule) ? mod : { "default": mod };
};
Object.defineProperty(exports, "__esModule", { value: true });
// @ts-ignore
const hyperaxe_1 = require("hyperaxe");
const getPageId_1 = __importDefault(require("./utils/getPageId"));
/**
 * @param
 * @return {NacaraTypes.MenuItem []}
 */
function flattenMenu(menu) {
    if (typeof menu === "string") {
        return [menu];
    }
    else if (typeof menu === "object") {
        if (menu.type === "link") {
            return [menu];
        }
        else if (menu.type === "section") {
            return menu.items.flatMap(flattenMenu);
        }
        else {
            throw "Invalid menu item element";
        }
    }
    else {
        throw "Invalid menu item element";
    }
}
/**
 * @param {NacaraTypes.MenuItem} pageIdOfPageToRender
 * @param {EleventyTypes.Page []} pages
 * @param {NacaraTypes.PageId} currentPageId
 */
function renderMenuItemPage(pageIdOfPageToRender, pages, currentPageId) {
    const pageOfMenuItem = pages.find((page) => (0, getPageId_1.default)(page.filePathStem) === pageIdOfPageToRender);
    if (!pageOfMenuItem) {
        throw `Could not find page with id ${pageIdOfPageToRender} in the pages collection`;
    }
    const isCurrentPage = (0, getPageId_1.default)(pageOfMenuItem?.filePathStem) === currentPageId;
    // console.log(isCurrentPage)
    console.log("currentPageId:", currentPageId);
    console.log("xx:", (0, getPageId_1.default)(pageOfMenuItem?.filePathStem));
    return (0, hyperaxe_1.li)((0, hyperaxe_1.a)({
        href: pageIdOfPageToRender,
        class: isCurrentPage ? "is-active" : "",
    }, pageOfMenuItem?.data.title));
}
/**
 *
 * @param menuItem
 * @param pages
 * @param currentPageId
 */
function renderSubMenu(menuItem, pages, currentPageId) {
    if (typeof menuItem === "string") {
        return renderMenuItemPage(menuItem, pages, currentPageId);
    }
    else if (typeof menuItem === "object") {
        if (menuItem.type === "link") {
            return (0, hyperaxe_1.li)((0, hyperaxe_1.a)({ href: menuItem.href }, menuItem.label));
        }
        else if (menuItem.type === "section") {
            // We don't support nested sections yet
            return null;
        }
    }
}
/**
 *
 * @param menu
 * @param pages
 * @param currentPageId
 */
function renderMenu(menu, pages, currentPageId) {
    const menuElements = menu.map((menuItem) => {
        if (typeof menuItem === "string") {
            return (0, hyperaxe_1.ul)({ class: "menu-list" }, renderMenuItemPage(menuItem, pages, currentPageId));
        }
        else if (typeof menuItem === "object") {
            if (menuItem.type === "link") {
                return (0, hyperaxe_1.ul)({ class: "menu-list" }, (0, hyperaxe_1.li)((0, hyperaxe_1.a)({ href: menuItem.href }, menuItem.label)));
            }
            else if (menuItem.type === "section") {
                return [
                    (0, hyperaxe_1.p)({ class: "menu-label" }, menuItem.label),
                    (0, hyperaxe_1.ul)({ class: "menu-list" }, menuItem.items.map((subMenuItem) => {
                        return renderSubMenu(subMenuItem, pages, currentPageId);
                    })),
                ];
            }
            else {
                throw "Invalid menu item element";
            }
        }
        else {
            throw "Invalid menu item element";
        }
    });
    return (0, hyperaxe_1.div)({ class: "menu-container" }, (0, hyperaxe_1.aside)({ class: "menu" }, menuElements));
}
/**
 *
 * @param pages
 * @returns
 */
module.exports = function (pages) {
    /**
     * @type {EleventyTypes.Page & NacaraTypes.PageData}
     */
    const currentPage = pages.find((page) => page.inputPath === this.ctx.page.inputPath);
    const currentPageId = (0, getPageId_1.default)(currentPage.filePathStem);
    console.log("-----");
    // const flatMenu = currentPage.data.menu.flatMap(flattenMenu);
    /**
     * @type {EleventyTypes.PageData & NacaraTypes.PageData}
     */
    const pageData = currentPage.data;
    if (pageData.menu) {
        return renderMenu(pageData.menu, pages, currentPageId).outerHTML;
    }
    else {
        return null;
    }
};
