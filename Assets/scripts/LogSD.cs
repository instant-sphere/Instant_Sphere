using System;
using System.IO;
using UnityEngine;

public sealed class LogSD
{

  public DateTime file_date;
  public String file_date_str;

  // RT=Real time; HQ = haut qualité
  public enum enum_state {RT, HQ};
  public enum_state state;


  public LogSD(){
    this.file_date = System.DateTime.Now;
    this.file_date_str = file_date.ToString("MM-dd-yyyy_HH.mm.ss");
    state = enum_state.RT;
  }

  public void new_date (){
      this.file_date = System.DateTime.Now;
    this.file_date_str = file_date.ToString("MM-dd-yyyy_HH.mm.ss");
  }

  public void WriteFile (string file, string to_print)
  {
    try
    {

      StreamWriter  monStreamWriter = new StreamWriter(@"./logs/"+file+".log",true);
      //StreamWriter  monStreamWriter = new StreamWriter(File.Create(fichier));

      //Ecriture du texte dans votre fichier
      monStreamWriter.WriteLine(to_print);

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
