using System;
using System.IO;
using UnityEngine;

public sealed class LogSD
{

  // public void creationFichier (string fichier){
  //   FileInfo mon_fichier = new FileInfo(fichier+".log");
  //   mon_fichier.Create();
  //   //mon_fichier.Close();
  // }

  public void ecritureFichier (string fichier)
  {
    try
    {

      StreamWriter  monStreamWriter = new StreamWriter(@"./logs/"+fichier+".log",true);
      //StreamWriter  monStreamWriter = new StreamWriter(File.Create(fichier));

      //Ecriture du texte dans votre fichier
      monStreamWriter.WriteLine("Ma toute première ligne ...");
      monStreamWriter.WriteLine("Ma seconde ligne ...");
      monStreamWriter.WriteLine("Ma troisième ligne ...");

      // Fermeture du StreamWriter (Très important)
      monStreamWriter.Close();
    }
    catch (Exception ex)
    {
      // Code exécuté en cas d'exception
      Debug.Log("Erreur lors de l'écriture du fichier" + ex.Message);
    }
  }
}
