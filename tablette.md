# Installation de l'application sur une nouvelle tablette

L'installation se déroule en deux phases: préparation et configuration de la tablette puis compilation et installation de l'application.

## Préparation de la tablette

### Rooter la tablette

1. Activer le mode développeur d'Android:
   - `Paramètres` > `A propos de la tablette` > `Informations sur le logiciel` 
   - Appuyer 5 fois sur `Numéro de version`
2. Dans les options de développement, activer `Déverrouillage OEM`
3. Eteindre la tablette
4. Appuyer simultanément sur les boutons `Home` `Power` et `Volume bas`
5. Valider en appuyant sur `Volume haut`
6. Brancher la tablette à un PC et lancer Odin
7. Cliquer sur `AP` et ouvrir le fichier CF-Auto-Root-XXXX.md5 correspondant à la tablette
8. Cliquer sur `Start`
9. Attendre que la tablette redémarre puis accepter l'effacement des données
10. Eteindre à nouveau la tablette, entrer en mode téléchargement et copier le fichier de root (points 4 à 8)
11. La tablette est rootée

### Configurer les options de la tablettes

- `Affichage`
  - Désactiver l'option `Luminosité automatique` et mettre la luminosité au maximum
  - `Mise en veille de l'écran` : 30 minutes
- `Ecran de verrouillage / Sécurité`
  - `Mode de déverrouillage` : Aucun
  - Activer l'option `Sources inconnues`
- `Options de développement`
  - Activer `Débogage USB`
  - Activer `Laisser données mobiles activées`
- Orienter la tablette en mode paysage puis désactiver l'option rotation automatique dans le menu en haut

### Installation des outils

Il faut installer les applications `Script Manager - SManager` et `Override DNS`

Pour `Override DNS`, sélectionner `Google` dans la liste des DNS et faire `Save` et `Apply`

Pour `SManager`:

- Ouvrir un terminal dans le dossier `platform-tools` du SDK android

- `./adb push chemin/vers/route.sh /sdcard`

- ```bash
  ./adb shell
  su
  cd /sdcard
  mv route.sh /data/knox/sdcard/
  ```

- Lancer `SManager` ouvrir le script `route.sh` et activer les options `Su`, `Boot` et `Net` puis faire `Enregistrer`

## Transfert sur la tablette

Pour compiler l'application sur la tablette: 

1. Ouvir le projet dans `Unity` 
2. Appuyer sur `Ctrl + B` 

 
