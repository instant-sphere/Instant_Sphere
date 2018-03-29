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
const LOGS_KIBANA_DIR = '/var/log/instant-sphere/kibana/';

var Storage = multer.diskStorage({
  destination: function (req, file, callback) {
    callback(null, '/var/log/instant-sphere/');
  },
  filename: function (req, file, callback) {
    callback(null, file.originalname);
  }
});

var upload = multer({ storage : Storage}).single('logUploader');

// writeKibanaLogs();

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
}).listen(PORT);*/

/**
* Returns formatted date
*/
function getDate() {
	var date = new Date()
	date = date.toUTCString().replace(/ /g,'_');
	return date.substring(5, date.length);
}

/**
* Saves daily logs in
*/
function saveLogs(data) {
	var file = getDate() + '.log';
	fs.writeFile(LOGS_DIR + file, data, function(err) {
		if (err) {
			console.log(err);
		}
		console.log("Saved logs in" + file);
	});
}

/**
* Generates formatted stats for Kibana
*/
function writeKibanaLogs() {
	var captures = {};
	var shareActions = {};
	var visualizeActions = {};

	var dir = __dirname + "/test/unity_logs/";
	fs.readdir(dir, function(err, files) {
		if (err) {
			console.log(err);
		}

		else {

			for (var i in files) {
				var file = files[i];

				// Treats only logs files
				if (file.substring(file.length - 4) == ".log") {
					var day = file.substring(0, 10);
					var log = fs.readFileSync(dir + file, 'utf8');
					var logJSON = JSON.parse(log);

					var nbCaptures = countEventOccurrences(logJSON, "capture");

					if (captures[day]) {
						captures[day] += nbCaptures;
					}

					else {
						captures[day] = nbCaptures;
					}

					shareActions[day] = countChoices(logJSON, "share", { "code,mail": 0, "facebook": 0, "abandon": 0 });
					visualizeActions[day] = countChoices(logJSON, "visualize", { "share": 0, "restart": 0, "abandon": 0 });
				}
			}

			console.log("Captures: %j", captures);
			console.log("Share actions %j", shareActions);
			console.log("Visualize actions %j", visualizeActions);

			saveCaptures(captures);
			saveChoices("visualize", visualizeActions);
			saveChoices("share", shareActions);
		}
	});
}

/**
* Saves formatted logs for Kibana --> Number of daily captures
*/
function saveCaptures(captures) {
	for (var day in captures) {
		var dailyCaptures = '';

		for (var i = 0; i<captures[day]; i++) {
			dailyCaptures += '{ "captures": 1 }\n';
		}

		var logFile = __dirname + '/test/kibana/capture/' + day + '.log';
		fs.writeFile(logFile, dailyCaptures, function(err) {
			if (err) {
				console.log(err);
			}
		});
	}
}

/**
* Saves formatted logs for Kibana --> Proportion of choices for a given event
*/
function saveChoices(eventName, choices) {
	for (var day in choices) {
		var res = '';

		for (var choice in choices[day]) {
			for (var j = 0; j<choices[day][choice]; j++) {
				res += '{ "' + eventName + '": "' + choice + '" }\n';
			}
		}
		var logFile = __dirname + '/test/kibana/' + eventName + '/' + day + '.log';
		fs.writeFile(logFile, res, function(err) {
			if (err) {
				console.log(err);
			}
		});
	}
}

/**
* Counts occurrences of choices for the event @eventName
*/
function countChoices(log, eventName, choices) {
	var entry = log["log_entry"];
	entry.forEach(function(e) {
		if (e["event"] == eventName) {
			var choice = e["choice"];
			choices[choice]++;
		}
	});
	return choices;
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
