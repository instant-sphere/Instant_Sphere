#!/usr/bin/env nodejs

const http = require('http');
var PORT = 2000;
http.createServer((request, response) => {
	var body = [];
	// Collects the data in a array
	request.on('data', (chunk) => {
		body.push(chunk);
	}).on('end', () => {
		// Then concatenates and stringifies it
		body = Buffer.concat(body).toString();
		console.log(decodeURIComponent(body.replace(/\+/g, "")));
		console.log("\n\n");
		response.end(body);
	});
}).listen(PORT);

console.log('Server running');
