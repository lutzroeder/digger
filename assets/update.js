#!/usr/bin/env node

const fs = require("fs");
const path = require("path");

const resourceMap = fs.readdirSync("./").filter(function(file) { return file != path.basename(__filename); }).map(function(file) {
    return "'" + file + "': '" + fs.readFileSync(file).toString("base64") + "'";
});

var data = fs.readFileSync("../digger.js", "utf-8");
data = data.replace(/(<!-- BEGIN RESOURCE MAP -->)([\s\S]*?)(<!-- END RESOURCE MAP -->)/gm, function (_, start, body, end) {
    return start + "\nvar resourceMap = {\n" + resourceMap.join(",\n") + "\n};\n" + end;
});
fs.writeFileSync("../digger.js", data);
