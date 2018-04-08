## Installer Nodejs ##
`curl-sL https://deb.nodesource.com/setup_8.x |sudo-Ebash-`
`sudo apt install -y nodejs`
`sudo npm install`

## Installer mongo databse ##
`sudo apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv 2930ADAE8CAF5059EE73BB4B58712A2291FA4AD5`

`echo "deb [ arch=amd64,arm64 ] https://repo.mongodb.org/apt/ubuntu xenial/mongodb-org/3.6 multiverse" | sudo tee /etc/apt/sources.list.d/mongodb-org-3.6.list`

`sudo apt-get update`

`sudo apt-get install -y mongodb-org`


## Créer le modèle  ##
Créer le fichier  `app/model/tablettes.js`
Code :

```js
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
```



## Installer Nginx ##
`sudo apt install nginx`

##Installer la pile ELK##
```bash
sudo apt install elasticsearch kibana logstash
sudo systemctl enable elasticsearch
sudo systemctl enable kibana
sudo systemctl enable logstash
```

**Ne pas oublier de copier les fichiers de configuration qui sont dans le répertoire `conf` aux bons emplacements.**

### Connexion à mongo

Revenir au dossier de l'application et créer `config.js`

Code :

```json
module.exports = {
'secret': 'clé_qui_servira_a_generer_token',
'database': 'mongodb://127.0.0.1:27017'
};
```


