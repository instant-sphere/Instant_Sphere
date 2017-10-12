using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LitJson;
using System.Text;

public class osc_controller : MonoBehaviour {

    HttpRequest mHTTP = new HttpRequest();
    Queue<MethodInfo> mExecutionQueue = new Queue<MethodInfo>();
    string mSessionId;
    bool mBusy = false;
    enum OSCStates { DISCONNECTED, IDLE, TAKE_PHOTO, DOWNLOAD_PHOTO, DELETE_PHOTO };
    OSCStates mCurrentState = OSCStates.DISCONNECTED;

    public Text debugger; 

    // Use this for initialization
    void Start ()
    {
        //mExecutionQueue.Enqueue(this.GetType().GetMethod("AskCameraInfo"));
        mExecutionQueue.Enqueue(this.GetType().GetMethod("AskStartSession"));
        mExecutionQueue.Enqueue(this.GetType().GetMethod("AskTakePicture"));
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (!mBusy && mExecutionQueue.Count > 0)
        {
            mExecutionQueue.Dequeue().Invoke(this, null);
            mBusy = true;
        }
        else if(mBusy && mHTTP.IsTerminated())
        {
            string s = mHTTP.GetHTTPResponse();
            ResponseHandler(s);
            mBusy = false;

            debugger.text = s;
            Debug.Log(s);
        }
    }

    private void ResponseHandler(string result)
    {
        JsonData jdata;
        try
        {
            jdata = HttpRequest.JSONStringToDictionary(result);
        }
        catch
        {
            jdata = null;
        }

        switch (mCurrentState)
        {
            case OSCStates.DISCONNECTED:    // It's a sessionStart reply
                mSessionId = jdata["results"]["sessionId"].ToString();
                mCurrentState = OSCStates.IDLE;
                break;
            case OSCStates.IDLE:
                // We shouldn't received a response from server when idle
                break;
            case OSCStates.TAKE_PHOTO:
                string state = jdata["state"].ToString();
                if (state == "inProgress")
                    mExecutionQueue.Enqueue(this.GetType().GetMethod("AskProgressStatus"));
                else if(state == "done")
                {
                    string URI = jdata["results"]["fileUrl"].ToString();
                    //TODO ask for download
                    mCurrentState = OSCStates.DOWNLOAD_PHOTO;
                }
                break;
            case OSCStates.DOWNLOAD_PHOTO:
                break;
            case OSCStates.DELETE_PHOTO:
                break;
            default:
                break;
        }
    }

    public void AskCameraInfo()
    {
        mHTTP.ChangeCommand(HttpRequest.Commands.GET_INFO);
        mHTTP.Execute();
    }

    public void AskStartSession()
    {
        mHTTP.ChangeCommand(HttpRequest.Commands.POST_C_EXECUTE);
        mHTTP.SetJSONData(ConstructStartSessionJSONString());
        mHTTP.Execute();
    }

    private string ConstructStartSessionJSONString()
    {
        StringBuilder sb = new StringBuilder();
        JsonWriter json = new JsonWriter(sb);
        json.WriteObjectStart();
        json.WritePropertyName("name");
        json.Write("camera.startSession");
        json.WriteObjectEnd();

        return sb.ToString();
    }

    public void AskTakePicture()
    {
        mHTTP.ChangeCommand(HttpRequest.Commands.POST_C_EXECUTE);
        mHTTP.SetJSONData(ConstructTakePictureJSONString());
        mCurrentState = OSCStates.TAKE_PHOTO;
        mHTTP.Execute();
    }

    private string ConstructTakePictureJSONString()
    {
        StringBuilder sb = new StringBuilder();
        JsonWriter json = new JsonWriter(sb);
        json.WriteObjectStart();
        json.WritePropertyName("name");
        json.Write("camera.takePicture");
        json.WritePropertyName("parameters");
        json.WriteObjectStart();
        json.WritePropertyName("sessionId");
        json.Write(mSessionId);
        json.WriteObjectEnd();
        json.WriteObjectEnd();

        return sb.ToString();
    }

    /*public byte[] TakePhoto()
    {
        byte[] img = { 1, 2, 3 };



        return img;
    }*/
}
