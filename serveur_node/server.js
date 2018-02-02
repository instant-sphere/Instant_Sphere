var Express = require('express');
 var multer = require('multer');
 var bodyParser = require('body-parser');
 var app = Express();
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
         callback(null, "pfa" + "_" + getDateTime() + "_" + file.originalname);
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
             return res.end("Something went wrong!");
         }
         console.log(req.files[0].filename);

         return res.render('affichage.ejs',{ fullUrl: req.protocol + '://' + req.get('host') + '/pictures/', 
nom_fichier: req.files[0].filename});
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

 app.listen(2000, function(a) {
     console.log("Listening to port 2000");
 });