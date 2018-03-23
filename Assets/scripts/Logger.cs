using System;
using System.IO;
using UnityEngine;
using LitJson;
using System.Text;

public sealed class Logger
{
    static readonly Logger mInstance = new Logger();
    string mFileDateStr;
    StringBuilder mSB = new StringBuilder();
    JsonWriter mJsonWriter;

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
            if (mInstance.mJsonWriter == null)
                mInstance.mJsonWriter = new JsonWriter(mInstance.mSB);
            return mInstance;
        }
    }

    public string GetCurrentFileName()
    {
        return mFileDateStr;
    }

    private string NewDate()
    {
        return DateTime.Now.ToString("dd-MM-yyyy_HH.mm.ss");
    }

    private string GetFileName()
    {
        return Application.persistentDataPath + "/" + mFileDateStr + ".log";
    }

    private void WriteFile()
    {
        try
        {
            StreamWriter streamWriter = new StreamWriter(GetFileName(), true);
            streamWriter.WriteLine(mSB.ToString());
            streamWriter.Close();
            mJsonWriter.Reset();
            mFileDateStr = null;
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
        {
            NewFile();
            mJsonWriter.WriteArrayStart();
        }
        mJsonWriter.WriteObjectStart();
        mJsonWriter.WritePropertyName("event");
        mJsonWriter.Write("error");
        mJsonWriter.WritePropertyName("time");
        mJsonWriter.Write(NewDate());
        mJsonWriter.WritePropertyName("state");
        mJsonWriter.Write((int)currentState);
        mJsonWriter.WriteObjectEnd();
        mJsonWriter.WriteArrayEnd();
        WriteFile();
    }

    public void WriteStart()
    {
        NewFile();
        mState = enum_state.RT;
        mJsonWriter.WriteArrayStart();
        mJsonWriter.WriteObjectStart();
        mJsonWriter.WritePropertyName("event");
        mJsonWriter.Write("start");
        mJsonWriter.WritePropertyName("time");
        mJsonWriter.Write(NewDate());
        mJsonWriter.WriteObjectEnd();
    }

    public void WriteCapture()
    {
        mJsonWriter.WriteObjectStart();
        mJsonWriter.WritePropertyName("event");
        mJsonWriter.Write("capture");
        mJsonWriter.WritePropertyName("time");
        mJsonWriter.Write(NewDate());
        mJsonWriter.WriteObjectEnd();
    }

    public void WriteVisualizeAbandon()
    {
        mJsonWriter.WriteObjectStart();
        mJsonWriter.WritePropertyName("event");
        mJsonWriter.Write("visualize");
        mJsonWriter.WritePropertyName("time");
        mJsonWriter.Write(NewDate());
        mJsonWriter.WritePropertyName("choice");
        mJsonWriter.Write("abandon");
        mJsonWriter.WriteObjectEnd();
        mJsonWriter.WriteArrayEnd();
        WriteFile();
    }

    public void WriteVisualizeRestart()
    {
        mJsonWriter.WriteObjectStart();
        mJsonWriter.WritePropertyName("event");
        mJsonWriter.Write("visualize");
        mJsonWriter.WritePropertyName("time");
        mJsonWriter.Write(NewDate());
        mJsonWriter.WritePropertyName("choice");
        mJsonWriter.Write("restart");
        mJsonWriter.WriteObjectEnd();
    }

    public void WriteVisualizeShare()
    {
        mJsonWriter.WriteObjectStart();
        mJsonWriter.WritePropertyName("event");
        mJsonWriter.Write("visualize");
        mJsonWriter.WritePropertyName("time");
        mJsonWriter.Write(NewDate());
        mJsonWriter.WritePropertyName("choice");
        mJsonWriter.Write("share");
        mJsonWriter.WriteObjectEnd();
    }

    public void WriteShareFacebook()
    {
        mJsonWriter.WriteObjectStart();
        mJsonWriter.WritePropertyName("event");
        mJsonWriter.Write("share");
        mJsonWriter.WritePropertyName("time");
        mJsonWriter.Write(NewDate());
        mJsonWriter.WritePropertyName("choice");
        mJsonWriter.Write("facebook");
        mJsonWriter.WriteObjectEnd();
    }

    public void WriteShareAbandon()
    {
        mJsonWriter.WriteObjectStart();
        mJsonWriter.WritePropertyName("event");
        mJsonWriter.Write("share");
        mJsonWriter.WritePropertyName("time");
        mJsonWriter.Write(NewDate());
        mJsonWriter.WritePropertyName("choice");
        mJsonWriter.Write("abandon");
        mJsonWriter.WriteObjectEnd();
        mJsonWriter.WriteArrayEnd();
        WriteFile();
    }

    public void WriteNavigate()
    {
        mJsonWriter.WriteObjectStart();
        mJsonWriter.WritePropertyName("event");
        if (mState == enum_state.RT)
            mJsonWriter.Write("navigate_RT");
        else
            mJsonWriter.Write("navigate_HD");
        mJsonWriter.WritePropertyName("time");
        mJsonWriter.Write(NewDate());
        mJsonWriter.WriteObjectEnd();
    }

    public void WriteTimeout()
    {
        mJsonWriter.WriteObjectStart();
        mJsonWriter.WritePropertyName("event");
        mJsonWriter.Write("timeout");
        mJsonWriter.WritePropertyName("time");
        mJsonWriter.Write(NewDate());
        mJsonWriter.WriteObjectEnd();
        mJsonWriter.WriteArrayEnd();
        WriteFile();
    }

    public void WriteShareCode()
    {
        mJsonWriter.WriteObjectStart();
        mJsonWriter.WritePropertyName("event");
        mJsonWriter.Write("share");
        mJsonWriter.WritePropertyName("time");
        mJsonWriter.Write(NewDate());
        mJsonWriter.WritePropertyName("choice");
        mJsonWriter.Write("code,mail");
        mJsonWriter.WriteObjectEnd();
    }
}
