var Express = require('express');
const process = require('process');
var scriptPath = process.argv[1];
var scriptDirectory = scriptPath.substring(0, scriptPath.lastIndexOf('/'));
process.chdir(scriptDirectory);
var multer = require('multer');
//const helmet = require('helmet');
const fs = require('fs');
var randtoken = require('rand-token').generator({chars: 'a-z'});
const https = require('http');
// const https_options = {
//   key: fs.readFileSync("../isphere.key"),
//   cert: fs.readFileSync("../certificate-593390.crt"),
// };
const basicAuth = require('express-basic-auth')

var bodyParser = require('body-parser');
var app = Express();
var RateLimit = require('express-rate-limit');

////AUTH
var bodyParser  = require('body-parser');
var morgan      = require('morgan');
var mongoose    = require('mongoose');

var jwt    = require('jsonwebtoken'); // used to create, sign, and verify tokens
var config = require('./config'); // get our config file
var Tablette   = require('./app/models/tablette.js'); // get our mongoose model


// =======================
// configuration =========
// =======================
console.log(mongoose.connect(config.database)); // connect to database
app.set('superSecret', config.secret); // secret variable


app.use(bodyParser.urlencoded({ extended: false }));
app.use(bodyParser.json());
////

/// infos console
app.use(morgan('dev'));
///

app.enable('trust proxy');

var limiter = new RateLimit({
    windowMs: 4*60*1000,
    max: 10,
    delayMs: 5*1000
});

const nodemailer = require('nodemailer');
var token_img;


  var Storage = multer.diskStorage({
     destination: function(req, file, callback) {
         callback(null, "./pictures");
     },
     filename: function(req, file, callback) {
        token_img = randtoken.generate(5);
         callback(null, token_img + ".jpg");
     }
 });

  var upload = multer({
     storage: Storage
 }).array("imgUploader", 3); //Field name and max count


//app.use(helmet());
//// =================
/// Partie authentification 
////=============

////// Les routes ci dessous ne sont pas protégées (jusqu'à apiRoutes.use(...))
///////
app.post('/enregistrement', function(req, res) {

  // create a sample user
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

  // save the sample user
  tab.save(function(err) {
    if (err) {
        res.json({ success: false, reason: "voir log" })
    } else{
    console.log('User saved successfully');
    res.json({ success: true });
}
  });
}});



/// Debut API
var apiRoutes = Express.Router(); 
apiRoutes.post('/demandetoken', function(req, res) {


  Tablette.findOne({
    id_tablette: req.body.id_tablette
  }, function(err, tablette) {

    if (err) throw err;
    console.log(tablette);
    if (!tablette) {

      res.json({ success: false, message: 'Tablette non enregistrée' });

    } else if (!tablette.autorisee) {
              res.json({ success: false, message: "Tablette non autorisée. Contactez l'administrateur pour l'activer" });
            }
            else if (tablette.already_given) {
                res.json({ success: false, message: "Token déjà délivré" });
            }

                else {

                const payload = {
                    id_tablette: req.body.id_tablette
                    };
                var token = jwt.sign(payload, app.get('superSecret'), {
        });
                tablette.already_given = true;
                tablette.save();

        // return the information including token as JSON
                res.json({
                  success: true,
                  message: 'Voici le joli token! ',
                  token: token
                });
      }   
    }
  );
});
//
//////// ==== Verification Token. ====
//////// Toutes les routes en dessous sont protégées
/////////
apiRoutes.use(function(req, res, next) {

    // check header or url parameters or post parameters for token
  var token = req.body.token || req.query.token || req.headers['x-access-token'];
  // decode token
  if (token) {

    // verifies secret and checks exp
    jwt.verify(token, app.get('superSecret'), function(err, decoded) {      
      if (err) {
        return res.json({ success: false, message: 'Failed to authenticate token.' });    
      } else {
        // if everything is good, save to request for use in other routes
        req.decoded = decoded;    
        console.log(req.decoded);
        next();
      }
    });

  } else {

    // if there is no token
    // return an error
    return res.status(403).send({ 
        success: false, 
        message: 'No token provided.' 
    });

  }
});


apiRoutes.get('/users', function(req, res) {
  Tablette.find({}, function(err, tablettes) {
    res.json(tablettes);
  });
});  

apiRoutes.post("/Upload", function(req, res) {
     upload(req, res, function(err) {
         if (err) {
            console.log(req);
             return res.end("Something went wrong!");
         }
         console.log(req);
         console.log(req.files[0].filename);
//          return res.render('affichage.ejs',{ fullUrl: req.protocol + '://' + req.get('host+ '/pictures/',
// nom_fichier: req.files[0].filename});
    return res.json({ status : 1, code : req.files[0].filename.substring(0, req.files[0].filename.lastIndexOf('.')) });
     });
 });


// Partage par Mail
var transporter = nodemailer.createTransport({
        service: "GandiMail",
        auth: {
            user: "noreply@instant-sphere.com",
            pass: "Eirb18:PFA"
        }
      }
);

//Send mail:


apiRoutes.post('/email', function(req, res, next) {
  var mail = req.body.mail;
  token_img = req.body.token_img;
  var mailOptions = {
          from: "noreply@instant-sphere.com",
          to: mail,
          subject: "instant-sphere",
          text: "Découvrez votre photo à 360° en vous connectant sur le site d'Instant \
          Sphere: https://server.instant-sphere.com/" + req.body.token_img,
          html: "Découvrez votre photo à 360° en vous connectant sur le site d'Instant \
          Sphere: https://server.instant-sphere.com/" + req.body.token_img
  };

  transporter.sendMail(mailOptions, function(error, info){
   if(error){
      return console.log(error);
   }
   console.log('Message sent: ' + info.response);
   return res.json({ status : 1, message : "email envoyé" });
});

transporter.close();
});

////// PARTIE MONITORING 
////// =======
const LOGS_DIR = '/var/log/instant-sphere/';
const LOGS_KIBANA_DIR = '/var/log/instant-sphere/logstash/';

var Storage_monitoring = multer.diskStorage({
  destination: function (req, file, callback) {
      // TODO check if it's normal logs or battery logs
    callback(null, '/var/log/instant-sphere/');
  },
  filename: function (req, file, callback) {
    callback(null, file.originalname);
  }
});

var upload_monitoring = multer({ storage : Storage_monitoring}).single('logUploader');


apiRoutes.post('/logs', function(req, res){
    upload_monitoring(req, res, function(err) {
        if(err) {
            return res.end("Error uploading file: " + err);
        }
        writeKibanaLogs();
        res.end("File is uploaded");
    });
});

apiRoutes.post('/hardware', function(req, res){
    var batteryLog = req.body.data;
    saveBattery(batteryLog);
});

app.use('/api', apiRoutes);

/////====
//// fin auth


app.get("/", function(req, res) {
  res.sendFile(__dirname + '/index.html');
 });

var adminRoute=Express.Router();

adminRoute.use(basicAuth({
    users: { 'admin': 'Eirb18' },
        challenge: true

}))

adminRoute.post('/activer',function(req,res){
    Tablette.findOne({
    id_tablette: req.body.id_tablette
  }, function(err, tablette) {
    console.log(tablette.already_given);

    tablette.autorisee = true;
    tablette.save();
    return res.redirect('/admin');
})});

adminRoute.post('/desactiver',function(req,res){
    Tablette.findOne({
    id_tablette: req.body.id_tablette
  }, function(err, tablette) {
    console.log("**statut :"+ tablette.already_given);

    tablette.autorisee = false;
    tablette.save();
    return res.redirect('/admin');
})});
adminRoute.get('/',function(req,res) {
    console.log("on rentre dans /admin");
    Tablette.find({}, function(err, tablettes) {
    console.log(tablettes);
        return res.render('admin.ejs',{ tablettes_liste: tablettes});

  });
});
app.use('/admin', adminRoute);


app.get('/:id', function(req, res) {
         return res.render('affichage.ejs',{ fullUrl: 'https://' + req.get('host') + '/pictures/',
nom_fichier: req.params.id + ".jpg"});
});



app.post('/supprimer_img', limiter, function(req, res, next) {
	console.log('**************' + req.headers.referer.substring(34,39));
      fs.unlink('/home/isphere/NodeJs_isphere/pictures/'+req.headers.referer.substring(34, 39)+'.jpg', function(error) {
      if (error) {
         console.log(req);
         return res.end("Something went wrong!");
        }
        console.log('Deleted ' + req.headers.referer.substring(34, 39));
        res.redirect(req.protocol + '://' + req.get('host'));
  });
});
app.get('/pictures/*', (req, res) => {
    res.sendFile(req.url, {root: './'})
});
app.get('/images/*', (req, res) => {
    res.sendFile(req.url, {root: './'})
1});
app.get('/assets/*', (req, res) => {
    res.sendFile(req.url, {root: './'})
});

https.createServer(app).listen(333, "127.0.0.1");

///////=======
/////// Monitoring functions



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
		    console.log('going to parse' + dir + file);
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

////======== END Monitoring Func======


