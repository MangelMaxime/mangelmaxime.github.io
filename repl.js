const h = require("hyperaxe");
const { div, h1, p } = require("hyperaxe");

let result = h("div")([
    h("h1", "Hello World")(),
    h("p", "This is a paragraph")(),
]);

const result2 = div([
    h1("Hello World"),
    p("This is a paragraph"),
]);

console.log(result.outerHTML);
console.log(result2.outerHTML);
