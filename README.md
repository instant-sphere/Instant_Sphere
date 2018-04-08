Le rapport de PFA ce trouve à la racine du dépôt.

#Installation#
Pour mettre en service une nouvelle tablette, suivre les instructions contenues dans *tablette.md*
Pour installer l'environnement de développement et compiler le projet, suivre les instructions contenues dans *environnement_dev.md*
Pour déployer le serveur, suivre les instructions contenues dans *serveur_node.md*

#Commandes#
##Serveur##
Le serveur utilise **pm2** pour fonctionner:

```bash
pm2 start server.js #pour lancer le serveur
pm2 stop server #pour arrêter le serveur
pm2 logs server #pour voir les logs du serveur en direct
pm2 show server #pour avoir des infos sur le serveur
pm2 monit #pour avoir des statistiques de performance
pm2 startup #pour installer le démon pm2 qui relance le serveur en cas de crash et au boot
```

Voir la doc de **pm2** pour plus de commandes : https://www.npmjs.com/package/pm2

##Kibana##
L'interface de Kibana est accessible à l'adresse https://server.instant-sphere.com/kibana
Connexion avec *user*=kibana et *mdp*=PFA_Eirb18

##Page d'administration des tablettes##
La page d'administration des tablettes est accessible à l'adresse https://server.instant-sphere.com/admin
Connexion avec *user*=admin et *mdp*=Eirb18

