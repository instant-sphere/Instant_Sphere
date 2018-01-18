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

    DateTime mDate = DateTime.Now;
    string nowStr;

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

    public void new_date(){
      mDate = DateTime.Now;
      nowStr = mDate.ToString("MM-dd-yyyy_HH.mm.ss");
    }

    public void write_timeout(screens_controller.ScreensStates currentState){
      new_date();
      WriteFile(mFileDataStr, "\t{\"event\": \"timeout\", \"time\": \"" + nowStr + "\", \"state\": \"" + currentState + "\"}");
      WriteFile(mFileDataStr, "]");
    }

    public void write_start(){
      state = LogSD.enum_state.RT;
      new_date();
      NewDate();
      WriteFile(mFileDataStr, "[");
      WriteFile(mFileDataStr, "\t{\"event\": \"start\", \"time\": \"" + nowStr + "\"},");
    }

    public void write_capture(){
      new_date();
      WriteFile(mFileDataStr, "\n\t{\"event\": \"capture\", \"time\": \"" + nowStr + "\"},");
    }

    public void write_visualize_abandon(){
      new_date();
      WriteFile(mFileDataStr, "\n\t{\"event\": \"visualize\", \"time\": \"" + nowStr + "\", \"choice\": \"abandon\"}");
      WriteFile(mFileDataStr, "]");
    }

    public void write_visualize_restart(){
      new_date();
      WriteFile(mFileDataStr, "\n\t{\"event\": \"visualize\", \"time\": \"" + nowStr + "\", \"choice\": \"restart\"},");
    }

    public void write_visualize_share(){
      new_date();
      WriteFile(mFileDataStr, "\n\t{\"event\": \"visualize\", \"time\": \"" + nowStr + "\", \"choice\": \"share\"},");
    }

    public void write_share_facebook(){
      new_date();
      WriteFile(mFileDataStr, "\n\t{\"event\": \"share\", \"time\": \"" + nowStr + "\", \"choice\": \"facebook\"},");
    }

    public void write_share_abandon(){
      new_date();
      WriteFile(mFileDataStr, "\n\t{\"event\": \"share\", \"time\": \"" + nowStr + "\", \"choice\": \"abandon\"}");
      WriteFile(mFileDataStr, "]");
    }

    public void write_navigate_RT(){
      new_date();
      WriteFile(mFileDataStr, "\t{\"event\": \"navigate_RT\", \"time\": \"" + nowStr + "\"},");
    }

    public void write_navigate_HD(){
      new_date();
      WriteFile(mFileDataStr, "\t{\"event\": \"navigate_HD\", \"time\": \"" + nowStr + "\"},");
    }
}
