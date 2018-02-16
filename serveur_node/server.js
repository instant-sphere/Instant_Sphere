var Express = require('express');
 var multer = require('multer');
 const helmet = require('helmet');
 const fs = require('fs');
var randtoken = require('rand-token');
const https = require('http');
const https_options = {
  key: fs.readFileSync("../isphere.key"),
  cert: fs.readFileSync("../certificate-593390.crt"),
}; 
 var bodyParser = require('body-parser');
 var app = Express();
 //app.use(helmet());

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
        var token = randtoken.generate(5);

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
//          return res.render('affichage.ejs',{ fullUrl: req.protocol + '://' + req.get('host') + '/pictures/', 
// nom_fichier: req.files[0].filename});
    return res.json({ status : 1, code : req.files[0].filename });
     });
 });

app.get('/pictures/*', (req, res) => {
    res.sendFile(req.url, {root: './'})
});
app.get('/images/*', (req, res) => {
    res.sendFile(req.url, {root: './'})
});
app.get('/assets/*', (req, res) => {
    res.sendFile(req.url, {root: './'})
});

https.createServer(app).listen(333);