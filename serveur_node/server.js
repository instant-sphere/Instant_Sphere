var Express = require('express');
 var multer = require('multer');
 //const helmet = require('helmet');
 const fs = require('fs');
var randtoken = require('rand-token').generator({
  chars: 'a-z'});
const https = require('http');
// const https_options = {
//   key: fs.readFileSync("../isphere.key"),
//   cert: fs.readFileSync("../certificate-593390.crt"),
// };
 var bodyParser = require('body-parser');
 var app = Express();
 //app.use(helmet());

const nodemailer = require('nodemailer');
var token;

 app.use(bodyParser.json());
function getDateTime() {

    var date = new Date();

    var hour = date.getHours();
    hour = (hour < 10 ? "0" : "") + hour;

    var min  = date.getMinutes();
    min = (min < 10 ? "0" : "") + min;

    var sec  = date.getSeconds();
    sec = (sec < 10 ? "0" : "") + sec;

    var year = date.getFullYear();

    var month = date.getMonth() + 1;
    month = (month < 10 ? "0" : "") + month;

    var day  = date.getDate();
    day = (day < 10 ? "0" : "") + day;

    return year + ":" + month + ":" + day + ":" + hour + ":" + min + ":" + sec;

}
  var Storage = multer.diskStorage({
     destination: function(req, file, callback) {
         callback(null, "./pictures");
     },
     filename: function(req, file, callback) {
        token = randtoken.generate(5);
        //var token = getDateTime();
         callback(null, token + ".jpg");
     }
 });

  var upload = multer({
     storage: Storage
 }).array("imgUploader", 3); //Field name and max count


app.get("/", function(req, res) {
  res.sendFile(__dirname + '/index.html');
 });

 app.post("/api/Upload", function(req, res) {
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

 app.get('/test_mail', function(req, res) {
 console.log(req);
          return res.render('test_mail.ejs');
 });

app.get('/:id', function(req, res) {
console.log(req);
         return res.render('affichage.ejs',{ fullUrl: req.protocol + '://' + req.get('host') + '/pictures/',
nom_fichier: req.params.id + ".jpg"});
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

app.post('/Email:mail', function(req, res, next) {
  console.log(req);

  var mailOptions = {
          from: "noreply@instant-sphere.com",
          to: req.params.mail,
          subject: "instant-sphere",
          text: token,
          html: "Découvrez votre photo à 360° en vous connectant sur le site d'Instant \
          Sphere: http://server.instant-sphere.com:333/ et entrez le code suivant:" + "<b>" + token + "</b>"
  };

  transporter.sendMail(mailOptions, function(error, info){
   if(error){
      return console.log(error);
   }
   console.log('Message sent: ' + info.response);
});

transporter.close();
});

app.post('/supprimer_img', function(req, res, next) {
      fs.unlink('D:\\Utilisateurs\\Jérémy\\Documents\\Programmation\\PFA\\free-instant-sphere-PFA\\serveur_node\\pictures\\'+req.headers.referer.substring(21, 25)+'.jpg', function(error) {
      if (error) {
         console.log(req);
         return res.end("Something went wrong!");
        }
        console.log('Deleted ' + req.headers.referer.substring(21, 25));
        res.redirect('http://server.instant-sphere.com/')
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
