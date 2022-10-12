const { DateTime } = require("luxon");

module.exports = function (date) {
    return DateTime.fromJSDate(date, { zone: "utc" }).toFormat(
        "yyyy-LL-dd HH:mm:ss"
    );
};
