var Express = require('express');

var multer = require('multer');
//const helmet = require('helmet');
const fs = require('fs');
var randtoken = require('rand-token').generator({chars: 'a-z'});
const https = require('http');
// const https_options = {
//   key: fs.readFileSync("../isphere.key"),
//   cert: fs.readFileSync("../certificate-593390.crt"),
// };
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

var apiRoutes = Express.Router(); 
 

apiRoutes.post('/demandetoken', function(req, res) {

  // find the user
    console.log(req.body.id_tablette);

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

apiRoutes.post("Upload", function(req, res) {
     upload(req, res, function(err) {
         if (err) {
            console.log(req);
             return res.end("Something went wrong!");
         }
         console.log(req);
         console.log(req.files[0].filename);
//          return res.render('affichage.ejs',{ fullUrl: req.protocol + '://' + req.get('host+ '/pictures/',
// nom_fichier: req.files[0].filename});
    return res.json({ status : 1, code : req.files[0].filename });
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

  var mailOptions = {
          from: "noreply@instant-sphere.com",
          to: mail,
          subject: "instant-sphere",
          text: token_img,
          html: "Découvrez votre photo à 360° en vous connectant sur le site d'Instant \
          Sphere: http://server.instant-sphere.com:333/ et entrez le code suivant: " + "<b>" + req.body.token_img + "</b>"
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


app.use('/api', apiRoutes);

/////====
//// fin auth



app.get("/", function(req, res) {
  res.sendFile(__dirname + '/index.html');
 });

 



app.get('/:id', function(req, res) {
console.log(req);
         return res.render('affichage.ejs',{ fullUrl: req.protocol + '://' + req.get('host') + '/pictures/',
nom_fichier: req.params.id + ".jpg"});
});



app.post('/supprimer_img', limiter, function(req, res, next) {
	console.log('**************' + req.headers.referer.substring(37,42));
      fs.unlink('/home/isphere/NodeJs_isphere/pictures/'+req.headers.referer.substring(37, 42)+'.jpg', function(error) {
      if (error) {
         console.log(req);
         return res.end("Something went wrong!");
        }
        console.log('Deleted ' + req.headers.referer.substring(37, 42));
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

https.createServer(app).listen(333);
