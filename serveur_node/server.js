/* Set working directory for relative paths */
const process = require('process');
process.chdir(process.env.PWD);

/* Modules */
const Express = require('express');
const RateLimit = require('express-rate-limit');
const multer = require('multer');
const fs = require('fs');
const basicAuth = require('express-basic-auth');
const bodyParser = require('body-parser');
const randtoken = require('rand-token').generator({ chars: 'a-z' });
const morgan = require('morgan');
const mongoose = require('mongoose');
const jwt = require('jsonwebtoken'); // used to create, sign, and verify tokens
const nodemailer = require('nodemailer');

const config = require('./config'); // get our config file
const Tablette = require('./app/models/tablette.js'); // get our mongoose model

/* Server */
var app = Express();

// =======================
// configuration =========
// =======================
console.log(mongoose.connect(config.database)); // connect to database
app.set('superSecret', config.secret); // secret variable
app.use(bodyParser.urlencoded({ extended: false }));
app.use(bodyParser.json());
////

/* For dev log */
//app.use(morgan('dev'));

app.enable('trust proxy');	// server is behind a Nginx reverse proxy

/* Requests limiter 10 per 4 minutes with 5 sec between each request */
var limiter = new RateLimit({
    windowMs: 4 * 60 * 1000,
    max: 10,
    delayMs: 5 * 1000
});

/* Stores photo and generates token */
var token_img;

var Storage = multer.diskStorage({
    destination: function (req, file, callback) {
        callback(null, "./pictures");
    },
    filename: function (req, file, callback) {
        token_img = randtoken.generate(5);
        callback(null, token_img + ".jpg");
    }
});

var upload = multer({
    storage: Storage
}).array("imgUploader", 3); //Field name and max count


// ====================
// Authentication part
// ====================

// Les routes ci dessous ne sont pas protegees (jusqu'a apiRoutes.use(...))
app.post('/enregistrement', function (req, res) {
    // create a new user
    if (!req.body.id_tablette) {
        res.json({ success: false });
    }
    else {
        var tab = new Tablette({
            id_tablette: req.body.id_tablette,
            nom: '',
            already_given: false,
            autorisee: false
        });

        // save the user
        tab.save(function (err) {
            if (err) {
                res.json({ success: false, reason: err })
            } else {
                console.log('User saved successfully');
                res.json({ success: true });
            }
        });
    }
});

/* API starts here */
var apiRoutes = Express.Router();
apiRoutes.post('/demandetoken', function (req, res) {
    Tablette.findOne({
        id_tablette: req.body.id_tablette
    }, function (err, tablet) {
        if (err)
            throw err;
        console.log(tablet);
        if (!tablet) {
            res.json({ success: false, message: 'Unregistered tablet.' });
        }
        else if (!tablet.autorisee) {
            res.json({ success: false, message: 'Unauthorized tablet. Contact the administrator to activate it.' });
        }
        else if (tablet.already_given) {
            res.json({ success: false, message: 'Token already issued' });
        }
        else {
            const payload = {
                id_tablette: req.body.id_tablette
            };
            var token = jwt.sign(payload, app.get('superSecret'), {
            });
            tablet.already_given = true;
            tablet.save();

            // return the information including token as JSON
            res.json({
                success: true,
                message: 'Here is the token',
                token: token
            });
        }
    }
    );
});

// ===================
// Verification Token
// ===================

// Toutes les routes en dessous sont protégées par le token
apiRoutes.use(function (req, res, next) {
    // check header or url parameters or post parameters for token
    var token = req.body.token || req.query.token || req.headers['x-access-token'];
    // decode token
    if (token) {
        // verifies secret and checks exp
        jwt.verify(token, app.get('superSecret'), function (err, decoded) {
            if (err) {
                return res.json({ success: false, message: 'Failed to authenticate token.' });
            }
            else {
                // if everything is good, save to request for use in other routes
                req.decoded = decoded;
                console.log("****VRIF TOKEN*****")
                console.log(decoded);
                Tablette.findOne({
                 id_tablette: req.decoded.id_tablette
                    }, function (err, tablet) {
                    if (tablet.autorisee){
                     next();
                 }
                 else {
                    return res.json({ success: false, message: 'Not allowed' });

                 }
            });
        }
    });
}
    else {
        // if there is no token
        // return an error
        return res.status(403).send({
            success: false,
            message: 'No token provided.'
        });
    }
});


apiRoutes.get('/users', function (req, res) {
    Tablette.find({}, function (err, tablettes) {
        res.json(tablettes);
    });
});

apiRoutes.post("/Upload", function (req, res) {
    upload(req, res, function (err) {
        if (err) {
            //console.log(req);
            return res.end("Something went wrong!");
        }
        //console.log(req);
        var filename = req.files[0].filename;
        //console.log(filename);

        var fileNoExtension = filename.substring(0, filename.lastIndexOf('.'));
        return res.json({ status: 1, code: fileNoExtension });
    });
});


// ===================
// Mail Sharing
// ===================
var transporter = nodemailer.createTransport({
    service: "GandiMail",
    auth: {
        user: "noreply@instant-sphere.com",
        pass: "Eirb18:PFA"
    }
}
);

//Send mail
apiRoutes.post('/email', function (req, res, next) {
    var mail = req.body.mail;
    token_img = req.body.token_img;
    var mailOptions = {
        from: 'noreply@instant-sphere.com',
        to: mail,
        subject: "instant-sphere",
        text: "Découvrez votre photo à 360° en vous connectant sur le site d'Instant \
          Sphere: https://server.instant-sphere.com/" + req.body.token_img,
        html: "Découvrez votre photo à 360° en vous connectant sur le site d'Instant \
          Sphere: <a href='https://server.instant-sphere.com/" + req.body.token_img + "'>https://server.instant-sphere.com/" + req.body.token_img + "</a>"
    };

    transporter.sendMail(mailOptions, function (error, info) {
        if (error) {
            console.log(error);
            return res.json({ status: 0, message: error });
        }
        console.log('Message sent: ' + info.response);
        return res.json({ status: 1, message: 'email sent' });
    });

    transporter.close();
});


// ===================
// ===================
// MONITORING PART
// ===================
// ===================

const LOGS_DIR = '/var/log/instant-sphere/';
const LOGS_KIBANA_DIR = '/var/log/instant-sphere/logstash/';

/* Stores log files send by tablets */
var Storage_monitoring = multer.diskStorage({
    destination: function (req, file, callback) {
        callback(null, LOGS_DIR);
    },
    filename: function (req, file, callback) {
        callback(null, file.originalname);
    }
});

var upload_monitoring = multer({ storage: Storage_monitoring }).single('logUploader');

apiRoutes.post('/logs', function (req, res) {
    upload_monitoring(req, res, function (err) {
        if (err) {
            return res.end("Error uploading file " + req.file.filename + ": " + err);
        }
        res.end(req.file.filename + " is uploaded");
        writeKibanaLogs(req.file.filename);
    });
});

apiRoutes.post('/hardware', function (req, res) {
    var batteryLog = req.body.data;
    saveBattery(batteryLog);
});

app.use('/api', apiRoutes);

// no more token protection


app.get("/", function (req, res) {
    res.sendFile(__dirname + '/index.html');
});

// =====================
// Administration pages
// =====================
var adminRoute = Express.Router();

adminRoute.use(basicAuth({
    users: { 'admin': 'Eirb18' },
    challenge: true
}))

adminRoute.post('/activer', function (req, res) {
    Tablette.findOne({
        id_tablette: req.body.id_tablette
    }, function (err, tablet) {
        console.log(tablet.already_given);

        tablet.autorisee = true;
        tablet.save();
        return res.redirect('/admin');
    })
});

adminRoute.post('/desactiver', function (req, res) {
    Tablette.findOne({
        id_tablette: req.body.id_tablette
    }, function (err, tablet) {
        console.log("**statut :" + tablet.already_given);

        tablet.autorisee = false;
        tablet.save();
        return res.redirect('/admin');
    })
});

adminRoute.get('/', function (req, res) {
    Tablette.find({}, function (err, tablets) {
        console.log(tablets);
        return res.render('admin.ejs', { tablettes_liste: tablets });
    });
});

app.use('/admin', adminRoute);

/* User 360 degrees viewer page */
app.get('/:id', function (req, res) {
    return res.render('affichage.ejs', { fullUrl: req.protocol + '://' + req.get('host') + '/pictures/', nom_fichier: req.params.id + ".jpg" });
});

/* Deletes a photo, protected by request limiter */
app.post('/supprimer_img', limiter, function (req, res, next) {
    var filename = req.headers.referer.substring(34, 39) + '.jpg';
    console.log('**************' + filename);
    fs.unlink('/home/isphere/NodeJs_isphere/pictures/' + filename, function (error) {
        if (error) {
            //console.log(req);
            return res.end("Something went wrong!");
        }
        console.log('Deleted ' + filename);
        res.redirect(req.protocol + '://' + req.get('host'));
    });
});

app.get('/pictures/*', (req, res) => {
    res.sendFile(req.url, { root: './' })
});

app.get('/images/*', (req, res) => {
    res.sendFile(req.url, { root: './' })
});

app.get('/assets/*', (req, res) => {
    res.sendFile(req.url, { root: './' })
});

// the server listen on localhost port 3333
app.listen(3333, "127.0.0.1");


// ====================================
// Monitoring and statistic functions
// ====================================

/**
* Returns formatted date
*/
function getDate() {
    var date = new Date()
    date = date.toUTCString().replace(/ /g, '_');
    return date.substring(5, date.length);
}

/**
* Saves daily logs
*/
function saveLogs(data) {
    var file = getDate() + '.log';
    fs.writeFile(LOGS_DIR + file, data, function (err) {
        if (err) {
            console.log(err);
        }
    });
}

/**
* Generates formatted stats for Kibana
*/
function writeKibanaLogs(file) {
    var captures = {};
    var shareActions = {};
    var visualizeActions = {};

    var dir = LOGS_DIR;

                // Treats only logs files
                if (file.substring(file.length - 4) == ".log") {
                    var day = file.substring(0, 10);
                    var log = fs.readFileSync(dir + file, 'utf8');
                    var logJSON = JSON.parse(log.trim());
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

            console.log("Captures: %j", captures);
            console.log("Share actions %j", shareActions);
            console.log("Visualize actions %j", visualizeActions);

            saveCaptures(captures);
            saveChoices("visualize", visualizeActions);
            saveChoices("share", shareActions);
}

/**
* Saves formatted logs for Kibana --> Number of daily captures
*/
function saveCaptures(captures) {
    for (var day in captures) {
        var dailyCaptures = '';

        for (var i = 0; i < captures[day]; i++) {
            dailyCaptures += '{ "captures": 1 }\n';
        }

        var logFile = LOGS_KIBANA_DIR + 'capture/' + day + '.log';
        fs.writeFile(logFile, dailyCaptures, function (err) {
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
            for (var j = 0; j < choices[day][choice]; j++) {
                res += '{ "' + eventName + '": "' + choice + '" }\n';
            }
        }

        var logFile = LOGS_KIBANA_DIR + eventName + '/' + day + '.log';
	var stream = fs.createWriteStream(logFile, {flags:'a'});
        stream.write(res);
	stream.end();
   }
}

/**
* Saves formatted logs for Kibana --> Camera battery and tablet battery per timestamp
*/
function saveBattery(batteryLog) {
    fs.appendFile(LOGS_KIBANA_DIR + 'battery/battery.log', batteryLog + '\n', function (err) {
        if (err) throw err;
    });
}

/**
* Counts occurrences of choices for the event @eventName
*/
function countChoices(log, eventName, choices) {
    log.forEach(function (e) {
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
        log.forEach(function (e) {
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
