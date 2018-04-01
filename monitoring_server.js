#!/usr/bin/env nodejs

const http = require('http');
const fs = require('fs');
const multer = require('multer');
const express = require('express');

const PORT = 334;
const LOGS_DIR = '/var/log/instant-sphere/';
const LOGS_KIBANA_DIR = '/var/log/instant-sphere/logstash/';

var app = express();

var Storage = multer.diskStorage({
    destination: function (req, file, callback) {
        // TODO check if it's normal logs or battery logs
        callback(null, '/var/log/instant-sphere/');
    },
    filename: function (req, file, callback) {
        callback(null, file.originalname);
    }
});

var upload = multer({storage : Storage}).single('logUploader');


app.post('/api/logs', function(req, res){
    upload(req, res, function(err) {
        if(err) {
            return res.end("Error uploading file: " + err);
        }
        writeKibanaLogs();
        res.end("File is uploaded");
    });
});

app.post('/api/hardware', function(req, res){
    var batteryLog = req.body.data;
    saveBattery(batteryLog);
});

app.listen(PORT, function() {
    console.log("Server running on port " + PORT);
});

/**
* Returns formatted date
*/
function getDate() {
    var date = new Date()
    date = date.toUTCString().replace(/ /g,'_');
    return date.substring(5, date.length);
}

function getTimestamp() {
    var date = new Date();
    return date.toUTCString().replace(/ /g,'_');
}

/**
* Saves daily logs
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

    var dir = LOGS_DIR;
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

        var logFile = LOGS_KIBANA_DIR + 'capture/' + day + '.log';
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
        var logFile = LOGS_KIBANA_DIR + eventName + '/' + day + '.log';
        fs.writeFile(logFile, res, function(err) {
            if (err) {
                console.log(err);
            }
        });
    }
}

/**
* Saves formatted logs for Kibana --> Camera battery and tablet battery per timestamp
*/
function saveBattery(batteryLog) {
    fs.appendFile(LOGS_KIBANA_DIR + 'battery/battery.log', batteryLog, function (err) {
        if (err) throw err;
    });
}

/**
* Counts occurrences of choices for the event @eventName
*/
function countChoices(log, eventName, choices) {
    log.forEach(function(e) {
        if (e["event"] == eventName) {
            var choice = e["choice"];
            choices[choice]++;
        }
    });
    return choices;
}

function countEventOccurrences(log, eventName) {
    var count = 0;

    if (log) {
        // Used to store non duplicate events
        var events = [];
        log.forEach(function(e) {
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
