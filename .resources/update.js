#!/usr/bin/env node

const fs = require('fs');
const path = require('path');

const resources = fs.readdirSync('./').filter((file) => file != path.basename(__filename)).map((file) => {
    return "'" + file + "': '" + fs.readFileSync(file).toString('base64') + "'";
});

const sourceFile = '../index.html';
let source = fs.readFileSync(sourceFile, 'utf-8');
source = source.replace(/(\/\/ BEGIN RESOURCE MAP)([\s\S]*?)(\/\/ END RESOURCE MAP)/gm, function (_, start, body, end) {
    return start + "\nvar resources = {\n" + resources.join(",\n") + "\n};\n" + end;
});
fs.writeFileSync(sourceFile, source);
