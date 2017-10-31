using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LitJson;
using System.Text;
using UnityEngine.Networking;

public class osc_controller : MonoBehaviour
{

    HttpRequest mHTTP = new HttpRequest();
    Queue<MethodInfo> mExecutionQueue = new Queue<MethodInfo>();
    string mSessionId;
    string mCurrentOperationId;
    string mFileURI;
    bool mBusy = false;
    enum OSCStates { DISCONNECTED, IDLE, TAKE_PHOTO, DOWNLOAD_PHOTO, DELETE_PHOTO };
    OSCStates mCurrentState = OSCStates.DISCONNECTED;

    // Use this for initialization
    void Start ()
    {
        //mExecutionQueue.Enqueue(this.GetType().GetMethod("AskCameraInfo"));
        mExecutionQueue.Enqueue(this.GetType().GetMethod("AskStartSession"));
        //mExecutionQueue.Enqueue(this.GetType().GetMethod("AskTakePicture"));

//         MeshRenderer mesh_renderer = mSphere.GetComponent<MeshRenderer>();
//         mesh_renderer.material.mainTexture = Resources.Load("test") as Texture;
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
            if(mHTTP.IsSuccessful())
            {
                ResponseHandler(s);
            }
                
            mBusy = false;

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
                {
                    mCurrentOperationId = jdata["id"].ToString();
                    mExecutionQueue.Enqueue(this.GetType().GetMethod("AskProgressStatus")); 
                }
                else if (state == "done")
                {
                    mFileURI = jdata["results"]["fileUri"].ToString();
                    mExecutionQueue.Enqueue(this.GetType().GetMethod("AskDownloadPhoto"));
                    mCurrentState = OSCStates.DOWNLOAD_PHOTO;
                }
                break;
            case OSCStates.DOWNLOAD_PHOTO:
                //byte[] bin = System.Text.Encoding.ASCII.GetBytes(result);

                //display.texture = mHTTP.GetTextureResponse();
                
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

    public void AskProgressStatus()
    {
        mHTTP.ChangeCommand(HttpRequest.Commands.POST_C_STATUS);
        mHTTP.SetJSONData(ConstructProgressStatusJSONString());
        mHTTP.Execute();
    }

    private string ConstructProgressStatusJSONString()
    {
        StringBuilder sb = new StringBuilder();
        JsonWriter json = new JsonWriter(sb);
        json.WriteObjectStart();
        json.WritePropertyName("id");
        json.Write(mCurrentOperationId);
        json.WriteObjectEnd();

        return sb.ToString();
    }

    public void AskDownloadPhoto()
    {
        mHTTP.ChangeCommand(mFileURI);
        mHTTP.Execute();
    }
}
