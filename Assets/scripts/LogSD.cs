using System;
using System.IO;
using UnityEngine;

public sealed class LogSD
{
    string mFileDateStr;

    // RT = Real Time; HQ = High Quality
    public enum enum_state { RT, HQ };
    enum_state mState;

    public LogSD()
    {
        NewDate();
        mState = enum_state.RT;
    }

    private void NewDate()
    {
        mFileDateStr = DateTime.Now.ToString("dd-MM-yyyy_HH.mm.ss");
    }

    private void WriteFile(string file, string toPrint)
    {
        try
        {
            StreamWriter streamWriter = new StreamWriter(Application.persistentDataPath + "/" + file + ".log", true);
            streamWriter.WriteLine(toPrint);
            streamWriter.Close();
        }
        catch (Exception ex)
        {
            Debug.Log("Error writing log file : " + ex.Message);
        }
    }

    public enum_state State()
    {
        return mState;
    }

    public void ChangeToRT()
    {
        mState = enum_state.RT;
    }

    public void ChangeToHQ()
    {
        mState = enum_state.HQ;
    }

    public void WriteTimeout(ScreensController.ScreensStates currentState)
    {
        NewDate();
        WriteFile(mFileDateStr, "\t{\"event\": \"timeout\", \"time\": \"" + mFileDateStr + "\", \"state\": \"" + currentState + "\"}");
        WriteFile(mFileDateStr, "]");
    }

    public void WriteStart()
    {
        mState = enum_state.RT;
        NewDate();
        WriteFile(mFileDateStr, "[");
        WriteFile(mFileDateStr, "\t{\"event\": \"start\", \"time\": \"" + mFileDateStr + "\"},");
    }

    public void WriteCapture()
    {
        NewDate();
        WriteFile(mFileDateStr, "\n\t{\"event\": \"capture\", \"time\": \"" + mFileDateStr + "\"},");
    }

    public void WriteVisualizeAbandon()
    {
        NewDate();
        WriteFile(mFileDateStr, "\n\t{\"event\": \"visualize\", \"time\": \"" + mFileDateStr + "\", \"choice\": \"abandon\"}");
        WriteFile(mFileDateStr, "]");
    }

    public void WriteVisualizeRestart()
    {
        NewDate();
        WriteFile(mFileDateStr, "\n\t{\"event\": \"visualize\", \"time\": \"" + mFileDateStr + "\", \"choice\": \"restart\"},");
    }

    public void WriteVisualizeShare()
    {
        NewDate();
        WriteFile(mFileDateStr, "\n\t{\"event\": \"visualize\", \"time\": \"" + mFileDateStr + "\", \"choice\": \"share\"},");
    }

    public void WriteShareFacebook()
    {
        NewDate();
        WriteFile(mFileDateStr, "\n\t{\"event\": \"share\", \"time\": \"" + mFileDateStr + "\", \"choice\": \"facebook\"},");
    }

    public void WriteShareAbandon()
    {
        NewDate();
        WriteFile(mFileDateStr, "\n\t{\"event\": \"share\", \"time\": \"" + mFileDateStr + "\", \"choice\": \"abandon\"}");
        WriteFile(mFileDateStr, "]");
    }

    public void WriteNavigateRT()
    {
        NewDate();
        WriteFile(mFileDateStr, "\t{\"event\": \"navigate_RT\", \"time\": \"" + mFileDateStr + "\"},");
    }

    public void WriteNavigateHD()
    {
        NewDate();
        WriteFile(mFileDateStr, "\t{\"event\": \"navigate_HD\", \"time\": \"" + mFileDateStr + "\"},");
    }
}
