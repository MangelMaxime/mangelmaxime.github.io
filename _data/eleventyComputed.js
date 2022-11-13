const fs = require('fs/promises');
const path = require('path');

async function fileExists(path) {
    try {
        await fs.access(path);
        return true;
    } catch {
        return false;
    }
}

module.exports = {
    menu: async data => {
        // Find the root of the project
        // Data doesn't contains the eleventyConfig.dir information
        // If needed, we can make the plugin expose it in the data
        const root = data.eleventy.env.root;

        //  Normal the path, so we can split using the path separator
        const normalizedInputPath = path.normalize(data.page.inputPath);
        // Extract all the segments of the path
        const inputPathSegments = normalizedInputPath.split(path.sep);
        // Build the section direction, which consist of the root + the first segment of the path
        const sectionDir = path.join(root, inputPathSegments[0])
        // Build the menu.json expected path
        const menuFilepath = path.join(sectionDir, 'menu.json');

        // If the menu.json file exists, read it and expose
        if (await fileExists(menuFilepath)) {
            const menu = await fs.readFile(menuFilepath);
            const parsedMenu = JSON.parse(menu);

            if (typeof parsedMenu === 'object') {
                return parsedMenu.items;
            } else if (typeof parsedMenu === 'array') {
                return parsedMenu;
            } else {
                throw new Error('Invalid menu.json file');
            }
        }

        // Otherwise, return null
        return null
    }
}
