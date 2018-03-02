#!/usr/bin/env nodejs

const http = require('http');
const fs = require('fs');
var PORT = 2000;
const LOGS_DIR = 'var/log/instant-sphere/';

http.createServer((request, response) => {
	var body = [];
	// Collects the data in a array
	request.on('data', (chunk) => {
		body.push(chunk);
	}).on('end', () => {
		// Then concatenates and stringifies it
		body = Buffer.concat(body).toString();
		var data = decodeURIComponent(body.replace(/\+/g, ""));
		data = data.substring(5, data.length); // removes "data="

		console.log("Received logs from Instant Sphere application");
		saveLogs(data);
		response.end(body);
	});
}).listen(PORT);

console.log('Server running');

function saveLogs(data) {
	var file = 'test.log';
	fs.writeFile(LOGS_DIR + file, data, function(err) {
		if (err) {
			console.log(err);
		}
	});
}
