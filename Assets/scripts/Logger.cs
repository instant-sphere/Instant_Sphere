using System;
using System.IO;
using UnityEngine;

public sealed class Logger
{
    static readonly Logger mInstance = new Logger();
    string mFileDateStr;

    // RT = Real Time; HQ = High Quality
    public enum enum_state { RT, HQ };
    enum_state mState;

    private Logger()
    {
        mState = enum_state.RT;
    }

    public static Logger Instance
    {
        get
        {
            return mInstance;
        }
    }

    private string NewDate()
    {
        return DateTime.Now.ToString("dd-MM-yyyy_HH.mm.ss");
    }

    private void WriteFile(string toPrint)
    {
        try
        {
            StreamWriter streamWriter = new StreamWriter(Application.persistentDataPath + "/" + mFileDateStr + ".log", true);
            streamWriter.WriteLine(toPrint);
            streamWriter.Close();
        }
        catch (Exception ex)
        {
            Debug.Log("Error writing log file : " + ex.Message);
        }
    }

    private void NewFile()
    {
        mFileDateStr = NewDate();
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

    public void WriteError(ScreensController.ScreensStates currentState)
    {
        if (mFileDateStr == null) // handle error logging on startup
            NewFile();
        WriteFile("\t{\"event\": \"error_state\", \"time\": \"" + NewDate() + "\", \"state\": \"" + currentState + "\"}");
        WriteFile("]");
    }

    public void WriteStart()
    {
        NewFile();
        mState = enum_state.RT;
        WriteFile("[");
        WriteFile("\t{\"event\": \"start\", \"time\": \"" + NewDate() + "\"},");
    }

    public void WriteCapture()
    {
        WriteFile("\n\t{\"event\": \"capture\", \"time\": \"" + NewDate() + "\"},");
    }

    public void WriteVisualizeAbandon()
    {
        WriteFile("\n\t{\"event\": \"visualize\", \"time\": \"" + NewDate() + "\", \"choice\": \"abandon\"}");
        WriteFile("]");
    }

    public void WriteVisualizeRestart()
    {
        WriteFile("\n\t{\"event\": \"visualize\", \"time\": \"" + NewDate() + "\", \"choice\": \"restart\"},");
    }

    public void WriteVisualizeShare()
    {
        WriteFile("\n\t{\"event\": \"visualize\", \"time\": \"" + NewDate() + "\", \"choice\": \"share\"},");
    }

    public void WriteShareFacebook()
    {
        WriteFile("\n\t{\"event\": \"share\", \"time\": \"" + NewDate() + "\", \"choice\": \"facebook\"},");
    }

    public void WriteShareAbandon()
    {
        WriteFile("\n\t{\"event\": \"share\", \"time\": \"" + NewDate() + "\", \"choice\": \"abandon\"}");
        WriteFile("]");
    }

    public void WriteNavigateRT()
    {
        WriteFile("\t{\"event\": \"navigate_RT\", \"time\": \"" + NewDate() + "\"},");
    }

    public void WriteNavigateHD()
    {
        WriteFile("\t{\"event\": \"navigate_HD\", \"time\": \"" + NewDate() + "\"},");
    }

    public void WriteTimeout()
    {
        WriteFile("\t{\"event\": \"timeout\", \"time\": \"" + NewDate() + "\"},");
        WriteFile("]");
    }
}
