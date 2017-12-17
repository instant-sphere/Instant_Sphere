using System;
using System.IO;
using UnityEngine;

public sealed class LogSD
{

    DateTime mFileDate;
    public string mFileDataStr;

    // RT = Real Time; HQ = High Quality
    public enum enum_state { RT, HQ };
    public enum_state state;


    public LogSD()
    {
        NewDate();
        state = enum_state.RT;
    }

    public void NewDate()
    {
        mFileDate = DateTime.Now;
        mFileDataStr = mFileDate.ToString("dd-MM-yyyy_HH.mm.ss");
    }

    public void WriteFile(string file, string toPrint)
    {
        try
        {
            StreamWriter streamWriter = new StreamWriter("/sdcard/" + file + ".log", true);

            streamWriter.WriteLine(toPrint);
            streamWriter.Close();
        }
        catch (Exception ex)
        {
            Debug.Log("Error writing log file : " + ex.Message);
        }
    }
}
