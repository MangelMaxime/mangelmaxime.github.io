// @ts-check

// @ts-ignore
import { div, h1, p, li, a, nav, ul, aside } from "hyperaxe";
import fs from "fs/promises";
import path from "path";
import { asyncFilterCallback } from "@11ty/eleventy";
import getPageId from "./utils/getPageId";

/**
 * @param
 * @return {NacaraTypes.MenuItem []}
 */
function flattenMenu(menu : MenuItem) : MenuItem [] {
    if (typeof menu === "string") {
        return [menu];
    } else if (typeof menu === "object") {
        if (menu.type === "link") {
            return [menu];
        } else if (menu.type === "section") {
            return menu.items.flatMap(flattenMenu);
        } else {
            throw "Invalid menu item element";
        }
    } else {
        throw "Invalid menu item element";
    }
}

/**
 * @param {NacaraTypes.MenuItem} pageIdOfPageToRender
 * @param {EleventyTypes.Page []} pages
 * @param {NacaraTypes.PageId} currentPageId
 */
function renderMenuItemPage(pageIdOfPageToRender : MenuItem, pages : any [], currentPageId : PageId) {
    const pageOfMenuItem = pages.find(
        (page) => getPageId(page.filePathStem) === pageIdOfPageToRender
    );

    if (!pageOfMenuItem) {
        throw `Could not find page with id ${pageIdOfPageToRender} in the pages collection`;
    }

    const isCurrentPage =
        getPageId(pageOfMenuItem?.filePathStem) === currentPageId;

    // console.log(isCurrentPage)
    console.log("currentPageId:", currentPageId)
    console.log("xx:", getPageId(pageOfMenuItem?.filePathStem))

    return li(
        a(
            {
                href: pageIdOfPageToRender,
                class: isCurrentPage ? "is-active" : "",
            },
            pageOfMenuItem?.data.title
        )
    );
}

/**
 *
 * @param menuItem
 * @param pages
 * @param currentPageId
 */
function renderSubMenu(menuItem : MenuItem, pages : any, currentPageId : PageId) {
    if (typeof menuItem === "string") {
        return renderMenuItemPage(menuItem, pages, currentPageId);
    } else if (typeof menuItem === "object") {
        if (menuItem.type === "link") {
            return li(a({ href: menuItem.href }, menuItem.label));
        } else if (menuItem.type === "section") {
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
function renderMenu(menu : Menu, pages : any, currentPageId : PageId) {
    const menuElements = menu.map((menuItem) => {
        if (typeof menuItem === "string") {
            return ul(
                { class: "menu-list" },
                renderMenuItemPage(menuItem, pages, currentPageId)
            );
        } else if (typeof menuItem === "object") {
            if (menuItem.type === "link") {
                return ul(
                    { class: "menu-list" },
                    li(a({ href: menuItem.href }, menuItem.label))
                );
            } else if (menuItem.type === "section") {
                return [
                    p({ class: "menu-label" }, menuItem.label),
                    ul(
                        { class: "menu-list" },
                        menuItem.items.map((subMenuItem) => {
                            return renderSubMenu(
                                subMenuItem,
                                pages,
                                currentPageId
                            );
                        })
                    ),
                ];
            } else {
                throw "Invalid menu item element";
            }
        } else {
            throw "Invalid menu item element";
        }
    });

    return div(
        { class: "menu-container" },
        aside({ class: "menu" }, menuElements)
    );
}

/**
 *
 * @param pages
 * @returns
 */
module.exports = function (this : any, pages : any []) {
    /**
     * @type {EleventyTypes.Page & NacaraTypes.PageData}
     */
    const currentPage = pages.find(
        (page) => page.inputPath === this.ctx.page.inputPath
    );

    const currentPageId = getPageId(currentPage.filePathStem);

    console.log("-----");
    // const flatMenu = currentPage.data.menu.flatMap(flattenMenu);

    /**
     * @type {EleventyTypes.PageData & NacaraTypes.PageData}
     */
    const pageData = currentPage.data;

    if (pageData.menu) {
        return renderMenu(pageData.menu, pages, currentPageId).outerHTML;
    } else {
        return null;
    }
};
