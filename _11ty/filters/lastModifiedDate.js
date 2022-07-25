const { promisify } = require('util');
const execFile = promisify(require('child_process').execFile);

// Cache the last modified date of a file
// because invoking git log is expensive
const lastModifiedDateCache = new Map();

async function lastModifiedDate(fileName) {
    try {
        const { stdout } = await execFile('git', ['--no-pager', 'log', '-1', '--format=%cd', fileName]);
        return new Date(stdout);
    } catch (e) {
        console.error(e.message);
        return new Date();
    }
}

module.exports = function (fileName, callback) {
    const cachedValue = lastModifiedDateCache.get(fileName);

    if (cachedValue) {
        callback(null, cachedValue);
    }
    else {
        lastModifiedDate(fileName).then((date) => {
            lastModifiedDateCache.set(fileName, date);
            callback(null, date);
        })
    }
}
