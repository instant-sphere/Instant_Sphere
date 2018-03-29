#!/usr/bin/env nodejs

const http = require('http');
const fs = require('fs');
//var PORT = 2000; // local conf
var PORT = 334;
const LOGS_DIR = '/var/log/instant-sphere/';
const LOGS_KIBANA_DIR = '/var/log/instant-sphere/kibana/';

// nbDailyCaptures();

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

		saveLogs(data);
		response.end(body);
	});
}).listen(PORT);

console.log('Server running');

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
* Number of daily captures
*/
function nbDailyCaptures() {
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
					shareActions[day] = shareAction(logJSON);
					visualizeActions[day] = visualizeAction(logJSON);
				}
			}

			console.log("Captures: %j", captures);
			console.log("Share actions %j", shareActions);
			console.log("Visualize actions %j", visualizeActions);

			for (var day in captures) {
				var content = '{ "dailyCaptures": ' + captures[day] + ', "shareActions": ' + JSON.stringify(shareActions[day]) + ', "visualizeActions": ' + JSON.stringify(visualizeActions[day]) + ' } \n';

				// var logFile = LOGS_KIBANA_DIR + day + '.log';
				var logFile = __dirname + '/test/kibana/' + day + '.log';
				fs.writeFile(logFile, content, function(err) {
					if (err) {
						console.log(err);
					}
				});
			}
		}
	});
}

function shareAction(log) {
	var entry = log["log_entry"];
	var actions = { "code,mail": 0, "facebook": 0, "abandon": 0 }
	entry.forEach(function(e) {
		if (e["event"] == "share") {
			var action = e["choice"];
			actions[action]++;
		}
	});
	return actions;
}

function visualizeAction(log) {
	var entry = log["log_entry"];
	var actions = { "share": 0, "restart": 0, "abandon": 0 };
	entry.forEach(function(e) {
		if (e["event"] == "visualize") {
			var action = e["choice"];
			actions[action]++;
		}
	});
	return actions;
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
