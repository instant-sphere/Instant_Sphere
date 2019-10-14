// get an instance of mongoose and mongoose.Schema
var mongoose = require('mongoose');
var Schema = mongoose.Schema;

// set up a mongoose model and pass it using module.exports
module.exports = mongoose.model('Tablette', new Schema({
    id_tablette: String,
    nom: String,
    already_given : Boolean,
    autorisee: Boolean
}));