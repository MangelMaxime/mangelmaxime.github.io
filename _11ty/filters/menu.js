// @ts-check

// @ts-ignore
const { div, h1, p, li, a, nav, ul, aside } = require("hyperaxe");
const fs = require("fs/promises");
const path = require("path");
const NacaraTypes = require("./../types/nacara");
const EleventyTypes = require("./../types/eleventy");
const getPageId = require("./utils/getPageId");

async function fileExists(path) {
    try {
        await fs.access(path);
        return true;
    } catch {
        return false;
    }
}

/**
 * @param {NacaraTypes.MenuItem} menu
 * @return {NacaraTypes.MenuItem []}
 */
function flattenMenu(menu) {
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
function renderMenuItemPage(pageIdOfPageToRender, pages, currentPageId) {
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
 * @param {NacaraTypes.MenuItem} menuItem
 * @param {EleventyTypes.Page []} pages
 * @param {NacaraTypes.PageId} currentPageId
 */
function renderSubMenu(menuItem, pages, currentPageId) {
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
 * @param {NacaraTypes.Menu} menu
 * @param {EleventyTypes.Page []} pages
 * @param {NacaraTypes.PageId} currentPageId
 */
function renderMenu(menu, pages, currentPageId) {
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
 * @this object
 * @param {object []} pages
 * @returns
 */
module.exports = function (pages) {
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
