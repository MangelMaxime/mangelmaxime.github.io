const menu = require("./posts/menu.json")

function flattenMenu(menu) {
    // If string
    if (typeof menu === "string") {
        return [ menu];
    // if object
    } else if (typeof menu === "object") {
        if (menu.type === "link") {
            return [ menu ];
        } else if (menu.type === "section") {
            return menu.items.flatMap(flattenMenu);
        }
    }
}

/**
 *
 * @param {string} pageId
 * @param {string []} acc
 * @param {object []} menuElements
 */
function generateBreadcrumb(pageId, acc, menuElements) {

    const [ currentMenuItem, ...restOfMenu] = menuElements;

    // There is no more menu to process, meaning we didn't find the pageId
    // Return nothing
    if (currentMenuItem === undefined) {
        return undefined;
    } else {
        if (typeof currentMenuItem === "string") {
            // This is the page we are looking for
            // Store the pageId in the accumulator and return the result
            if (currentMenuItem === pageId) {
                return [...acc, currentMenuItem];
            // Keep looking
            } else {
                return generateBreadcrumb(pageId, acc, restOfMenu);
            }
        } else if (typeof currentMenuItem === "object") {
            // A link cannot beling to the breadcrumb, so we skip it
            if (currentMenuItem.type === "link") {
                return generateBreadcrumb(pageId, acc, restOfMenu);
            } else if (currentMenuItem.type === "section") {
                const sectionResult = generateBreadcrumb(pageId, [...acc, currentMenuItem.label], currentMenuItem.items);

                // If the current section doesn't contain the pageId, we keep looking
                if (sectionResult === undefined) {
                    return generateBreadcrumb(pageId, acc, restOfMenu);
                // We got a result, so we store the section title in the accumulator
                // and return the result
                } else {
                    return sectionResult;
                }
            }
        }
    }
}

const res = generateBreadcrumb("guides/create-a-page", [], menu);

console.log(res)
