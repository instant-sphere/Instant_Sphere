using System;
using System.IO;
using UnityEngine;
using LitJson;
using System.Text;

/**
* This class handle log system
* It's a singleton
**/
public sealed class Logger
{
    static readonly Logger mInstance = new Logger();    // unique instance
    string mFileDateStr;    // timestamp (and name) of the current log file
    StringBuilder mSB = new StringBuilder();
    JsonWriter mJsonWriter;     //JSON writer object

    // RT = Real Time; HQ = High Quality
    public enum ELoggerState { RT, HQ };
    ELoggerState mState;

    /**
     * Private constructor
     **/
    private Logger()
    {
        mState = ELoggerState.RT;
    }

    /**
     * Gets the unique instance
     **/
    public static Logger Instance
    {
        get
        {
            if (mInstance.mJsonWriter == null)
                mInstance.mJsonWriter = new JsonWriter(mInstance.mSB);
            return mInstance;
        }
    }

    /**
     * Returns the name of the current log file
     **/
    public string GetCurrentFileName()
    {
        return mFileDateStr;
    }

    /**
     * Returns the current date and time as string
     **/
    private string NewDate()
    {
        return DateTime.Now.ToString("dd-MM-yyyy_HH.mm.ss");
    }

    /**
     * Returns the full name (including path) of the current log file
     **/
    private string GetFilePathAndName()
    {
        return Application.persistentDataPath + "/" + GetCurrentFileName() + ".log";
    }

    /**
     * Writes data from the JSON writer to the current log file
     **/
    private void WriteFile()
    {
        try
        {
            StreamWriter streamWriter = new StreamWriter(GetFilePathAndName(), true);
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

    /**
     * Generates a new name for the current log file
     **/
    private void NewFile()
    {
        mFileDateStr = NewDate();
    }

    /**
     * Returns the state RT or HQ
     **/
    public ELoggerState State()
    {
        return mState;
    }

    /**
     * Changes the state to Real Time
     **/
    public void ChangeToRT()
    {
        mState = ELoggerState.RT;
    }

    /**
     * Changes the state to High Quality
     **/
    public void ChangeToHQ()
    {
        mState = ELoggerState.HQ;
    }

    /**
     * Writes an error event in the log
     **/
    public void WriteError(ScreensController.ScreensStates currentState)
    {
        if (mFileDateStr == null) // handle error logging on startup screen
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

    /**
     * Writes a start event in the log
     **/
    public void WriteStart()
    {
        NewFile();
        mState = ELoggerState.RT;
        mJsonWriter.WriteArrayStart();
        mJsonWriter.WriteObjectStart();
        mJsonWriter.WritePropertyName("event");
        mJsonWriter.Write("start");
        mJsonWriter.WritePropertyName("time");
        mJsonWriter.Write(NewDate());
        mJsonWriter.WriteObjectEnd();
    }

    /**
     * Writes a capture event in the log
     **/
    public void WriteCapture()
    {
        mJsonWriter.WriteObjectStart();
        mJsonWriter.WritePropertyName("event");
        mJsonWriter.Write("capture");
        mJsonWriter.WritePropertyName("time");
        mJsonWriter.Write(NewDate());
        mJsonWriter.WriteObjectEnd();
    }

    /**
     * Writes the common part of visualize events
     **/
    private void WriteVisualizeCommon()
    {
        mJsonWriter.WriteObjectStart();
        mJsonWriter.WritePropertyName("event");
        mJsonWriter.Write("visualize");
        mJsonWriter.WritePropertyName("time");
        mJsonWriter.Write(NewDate());
    }

    /**
     * Writes a visualize event with choice = abandon in the log
     **/
    public void WriteVisualizeAbandon()
    {
        WriteVisualizeCommon();
        mJsonWriter.WritePropertyName("choice");
        mJsonWriter.Write("abandon");
        mJsonWriter.WriteObjectEnd();
        mJsonWriter.WriteArrayEnd();
        WriteFile();
    }

    /**
     * Writes a visualize event with choice = restart in the log
     **/
    public void WriteVisualizeRestart()
    {
        WriteVisualizeCommon();
        mJsonWriter.WritePropertyName("choice");
        mJsonWriter.Write("restart");
        mJsonWriter.WriteObjectEnd();
    }

    /**
     * Writes a visualize event with choice = share in the log
     **/
    public void WriteVisualizeShare()
    {
        WriteVisualizeCommon();
        mJsonWriter.WritePropertyName("choice");
        mJsonWriter.Write("share");
        mJsonWriter.WriteObjectEnd();
    }

    /**
     * Writes the common part of share events
     **/
    private void WriteShareCommon()
    {
        mJsonWriter.WriteObjectStart();
        mJsonWriter.WritePropertyName("event");
        mJsonWriter.Write("share");
        mJsonWriter.WritePropertyName("time");
        mJsonWriter.Write(NewDate());
    }

    /**
     * Writes a share event with choice = Facebook in the log
     **/
    public void WriteShareFacebook()
    {
        WriteShareCommon();
        mJsonWriter.WritePropertyName("choice");
        mJsonWriter.Write("facebook");
        mJsonWriter.WriteObjectEnd();
    }

    /**
     * Writes a share event with choice = code,mail in the log
     **/
    public void WriteShareCode()
    {
        WriteShareCommon();
        mJsonWriter.WritePropertyName("choice");
        mJsonWriter.Write("code,mail");
        mJsonWriter.WriteObjectEnd();
    }

    /**
     * Writes a share event with choice = abandon in the log
     **/
    public void WriteShareAbandon()
    {
        WriteShareCommon();
        mJsonWriter.WritePropertyName("choice");
        mJsonWriter.Write("abandon");
        mJsonWriter.WriteObjectEnd();
        mJsonWriter.WriteArrayEnd();
        WriteFile();
    }

    /**
     * Writes a navigate event in the log
     **/
    public void WriteNavigate()
    {
        mJsonWriter.WriteObjectStart();
        mJsonWriter.WritePropertyName("event");
        if (mState == ELoggerState.RT)
            mJsonWriter.Write("navigate_RT");
        else
            mJsonWriter.Write("navigate_HD");
        mJsonWriter.WritePropertyName("time");
        mJsonWriter.Write(NewDate());
        mJsonWriter.WriteObjectEnd();
    }

    /**
     * Write a timeout event in the log
     **/
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

    /**
     * Write a facebook success share event in the log
     **/
    public void WriteFacebookSuccess()
    {
        mJsonWriter.WriteObjectStart();
        mJsonWriter.WritePropertyName("event");
        mJsonWriter.Write("facebook success");
        mJsonWriter.WritePropertyName("time");
        mJsonWriter.Write(NewDate());
        mJsonWriter.WriteObjectEnd();
    }

    /**
     * Write a goodbye event in the log
     **/
    public void WriteGoodbye()
    {
        mJsonWriter.WriteObjectStart();
        mJsonWriter.WritePropertyName("event");
        mJsonWriter.Write("goodbye");
        mJsonWriter.WritePropertyName("time");
        mJsonWriter.Write(NewDate());
        mJsonWriter.WriteObjectEnd();
        mJsonWriter.WriteArrayEnd();
        WriteFile();
    }
}
