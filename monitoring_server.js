#!/usr/bin/env nodejs
const http = require('http');
const fs = require('fs');
const multer = require('multer');
const express = require('express');
var app = express();
//var PORT = 2000;
// local conf
const PORT = 334;
const LOGS_DIR = '/var/log/instant-sphere/';

var Storage = multer.diskStorage({
  destination: function (req, file, callback) {
    callback(null, '/var/log/instant-sphere/');
  },
  filename: function (req, file, callback) {
    callback(null, file.originalname);
  }
});

var upload = multer({ storage : Storage}).single('logUploader');

app.post('/',function(req,res){
    upload(req,res,function(err) {
        if(err) {
            return res.end("Error uploading file: " + err);
        }
        res.end("File is uploaded");
    });
});
app.listen(PORT, function() {
	console.log("Server running on port " + PORT);
});
/*http.createServer((request, response) => {
	var body = [];
	// Collects the data in a array
	request.on('data', (chunk) => {
		body.push(chunk);
	}).on('end', () => {
		// Then concatenates and stringifies it
		body = Buffer.concat(body).toString();
		var data = decodeURIComponent(body.replace(/\+/g, ""));
		data = data.substring(5, data.length); // removes "data="
		saveLogs(data);
		response.end(body);
	});
}).listen(PORT);
console.log('Server running');*/
function saveLogs(data) {
	var date = new Date();
	var file = date.toUTCString().replace(/ /g,'_');
	file = file.substring(5, file.length) + '.log';
	fs.writeFile(LOGS_DIR + file, data, function(err) {
		if (err) {
			console.log(err);
		}
		console.log("Saved logs in" + file);
	});
}

function countEventOccurrences(log, eventName) {
	var count = 0;
	var entry = log["log_entry"];
	if (entry) {
		// Used to store non duplicate events
		var events = [];
		entry.forEach(function(e) {
			if (e["event"] == eventName && !events.some(k => isEquivalent(k, e))) {
				events.push(e);
				count++;
			}
		});
	}
	return count;
}

function isEquivalent(a, b) {
	// Create arrays of property names
	var aProps = Object.getOwnPropertyNames(a);
	var bProps = Object.getOwnPropertyNames(b);
	// If number of properties is different,
	// objects are not equivalent
	if (aProps.length != bProps.length) {
		return false;
	}
	for (var i = 0; i < aProps.length; i++) {
		var propName = aProps[i];
		// If values of same property are not equal,
		// objects are not equivalent
		if (a[propName] !== b[propName]) {
			return false;
		}
	}
	// If we made it this far, objects
	// are considered equivalent
	return true;
}
