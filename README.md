<img src="https://www.fondation.univ-bordeaux.fr/wp-content/uploads/2017/12/2017-11-XP-ST-JEAN-instant-sphere-144x168.jpg"
 alt="Instant Sphere" title="Instant Sphere" align="left" />

# Instant Sphere: Self-service 360° photo booth

[<img src="http://files.gandi.ws/ba/ef/baefb23c-f436-49f3-b68d-c74a33515a39.jpg" alt="First prototype" title="First prototype" align="right" width="300" />](http://files.gandi.ws/ba/ef/baefb23c-f436-49f3-b68d-c74a33515a39.jpg)
 
**Instant Sphere** uses Ricoh Theta S and Samsung Galaxy Tab tablet to make a 360° self-service booth.
360° images taken by visitors can be uploaded to Facebook or on a custom hosting server.

The opposite prototype has been produced by students from ENSEIRB-MATMECA and ENSAM, funded by SNCF and demonstrated during 6 months in the main train station of Bordeaux, France.

Check out the videos of [the production](https://twitter.com/Instant_Sphere/status/957045965835337728), [the use](https://twitter.com/Instant_Sphere/status/958348433345187840) and a [press release (FR)](http://www.fondation.univ-bordeaux.fr/6229-2).

Technologies: *Unity, Android, NodeJS, nginx, Open Spherical Camera, Facebook developer*

## Getting started
### Client side: the camera booth
#### Prerequisites
* Electronics hardware requires :
  * Rooted *Samsung Galaxy Tab A6 10'1" 2016*: other Android tablets with the same resolution should suit well
  * *Ricoh Theta S*: other OSC compatible cameras should suit well, except Ricoh-specific commands that should be disabled
  * Some SIM card to be inserted in the tablet for sending the pictures over the Web
* Since Wifi connection is used to connect to the camera, 3G is needed to connect to Internet
* Android needs a root access to overwrite routing tables, by switching on both Wifi and 3G both at the same time
* If you intend to use the Facebook sharing feature, a Facebook developer account is needed and you must generate your own Facebook app

#### Version notes
In [v2.x](releases/tag/2.0) (branch `master`), a working server is compulsory, since the client app only runs if the tablet has been declared within the online administration page.
If you don't want a server, see [v1.x](releases/tag/1.4) (branch `fb_only`), but this version only supports Facebook sharing.
User Interfaces are in French.

#### Installation
1. Setup your environment development and compile the Unity app by following [the procedure (FR)](environnement_dev.md).
2. Install the software on a new tablet by following [the procedure (FR)](tablette.md).
3. At first statup, the tablet needs to be [declared to the server](https://server.instant-sphere.com/admin) and Wifi must be connected to the Theta S

### Server side: picture-hosting and statistics server
#### Prerequisites
* Software assumes that the [server](serveur_node) is running on `server.instant-sphere.com`, you can replace target by `localhost` or any other one
* Server needs [SMTP credentials](serveur_node/server.js#L201-L208) to send e-mails to (default credentials are void)


#### Installation
Follow instructions of [serveur_node](serveur_node.md) to setup the server.

#### Usage
Server can be driven by `pm2` :
```bash
pm2 start server.js
pm2 stop server
pm2 logs server    # See live logs
pm2 show server    # Get information
pm2 monit          # Performance statistics
pm2 startup        # Setup pm2 daemon launching the server at boot time
```

* Declare or disable the booth on the administration page: https://server.instant-sphere.com/admin
* Consult statistics on Kibana: https://server.instant-sphere.com/kibana
