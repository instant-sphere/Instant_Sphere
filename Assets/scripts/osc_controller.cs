using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LitJson;
using System;
using System.Text;
using UnityEngine.Networking;

public sealed class osc_controller : MonoBehaviour
{

    HttpRequest mHTTP = new HttpRequest();
    Queue<MethodInfo> mExecutionQueue = new Queue<MethodInfo>();
    string mSessionId;  //When starting only
    string mCurrentOperationId;
    string mFileURL;
    bool mBusy = false;
    enum OSCStates { DISCONNECTED, IDLE, TAKE_PHOTO, DOWNLOAD_PHOTO, DELETE_PHOTO };
    OSCStates mCurrentState;
    Action mCallBack = null;
    Material mDefaultSkybox;

    enum OSCActions { START_SESSION = 0, UPGRADE_API, SET_OPTIONS, TAKE_PICTURE, DOWNLOAD, PROGRESS_STATUS, CAMERA_INFO, DELETE};
    string[] mActionsMethodName = { "AskStartSession", "AskUpgradeAPI", "AskSetOptions", "AskTakePicture", "AskDownloadPhoto", "AskProgressStatus", "AskCameraInfo", "AskDeletePhoto"};

    // Use this for initialization
    void Start ()
    {
        mDefaultSkybox = RenderSettings.skybox;
        mCurrentState = OSCStates.DISCONNECTED;
        EnqueueAction(OSCActions.START_SESSION);
    }

    public void StartCapture(Action callback)
    {
        EnqueueAction(OSCActions.TAKE_PICTURE);
        mCallBack = callback;
    }

    public void resetSkybox()
    {
        RenderSettings.skybox = mDefaultSkybox;
    }

    private void EnqueueAction(OSCActions action)
    {
        string methodName = mActionsMethodName[(int)action];
        MethodInfo mi = this.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
        mExecutionQueue.Enqueue(mi);
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
                EnqueueAction(OSCActions.UPGRADE_API);
                EnqueueAction(OSCActions.SET_OPTIONS);
                mCurrentState = OSCStates.IDLE;
                break;
            case OSCStates.IDLE:
                break;
            case OSCStates.TAKE_PHOTO:
                string state = jdata["state"].ToString();
                if (state == "inProgress")
                {
                    mCurrentOperationId = jdata["id"].ToString();
                    EnqueueAction(OSCActions.PROGRESS_STATUS);
                }
                else if (state == "done")
                {
                    mFileURL = jdata["results"]["fileUrl"].ToString();
                    EnqueueAction(OSCActions.DOWNLOAD);
                    mCurrentState = OSCStates.DOWNLOAD_PHOTO;
                }
                break;
            case OSCStates.DOWNLOAD_PHOTO:
                byte[] image = mHTTP.GetRawResponse();
                if(mHTTP.GetRawResponse() != null)
                {
                    if(!System.IO.Directory.Exists("/sdcard/AEI"))
                        System.IO.Directory.CreateDirectory("/sdcard/AEI");
                    System.IO.File.WriteAllBytes("/sdcard/AEI/downloaded_image.jpg", image);

                    byte[] loadedPicture = System.IO.File.ReadAllBytes("/sdcard/AEI/downloaded_image.jpg");
                    Texture2D t = new Texture2D(2, 2);
                    t.LoadImage(loadedPicture);

                    // TODO : convert equirectangular to cubemap and don't create a new skybox from the shader "Skybox/Equirectangular"
                    /*Cubemap cmap = new Cubemap(t.height, TextureFormat.RGB24, false);
                    cmap.SetPixels(t.GetPixels(), CubemapFace.PositiveX);
                    cmap.filterMode = FilterMode.Trilinear;
                    cmap.Apply();*/

                    Material m = new Material(Shader.Find("Skybox/Equirectangular"));
                    m.SetTexture("_Tex", t);

                    RenderSettings.skybox = m;

                    EnqueueAction(OSCActions.DELETE);
                    mCurrentState = OSCStates.DELETE_PHOTO;
                }                
                break;
            case OSCStates.DELETE_PHOTO:
                if (mCallBack != null)
                {
                    mCallBack();
                    mCallBack = null;
                }
                mCurrentState = OSCStates.IDLE;
                break;
        }
    }

    private void AskCameraInfo()
    {
        mHTTP.ChangeCommand(HttpRequest.Commands.GET_INFO);
        mHTTP.Execute();
    }

    private void AskStartSession()
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

    private void AskUpgradeAPI()
    {
        mHTTP.ChangeCommand(HttpRequest.Commands.POST_C_EXECUTE);
        mHTTP.SetJSONData(ConstructUpgradeAPIJSONString());
        mHTTP.Execute();
    }

    private string ConstructUpgradeAPIJSONString()
    {
        StringBuilder sb = new StringBuilder();
        JsonWriter json = new JsonWriter(sb);
        json.WriteObjectStart();
        json.WritePropertyName("name");
        json.Write("camera.setOptions");
        json.WritePropertyName("parameters");
        json.WriteObjectStart();
        json.WritePropertyName("sessionId");
        json.Write(mSessionId);
        json.WritePropertyName("options");
        json.WriteObjectStart();
        json.WritePropertyName("clientVersion");
        json.Write(2);
        json.WriteObjectEnd();
        json.WriteObjectEnd();
        json.WriteObjectEnd();

        return sb.ToString();
    }

    private void AskSetOptions()
    {
        mHTTP.ChangeCommand(HttpRequest.Commands.POST_C_EXECUTE);
        mHTTP.SetJSONData(ConstructSetOptionsJSONString());
        mHTTP.Execute();
    }

    private string ConstructSetOptionsJSONString()
    {
        StringBuilder sb = new StringBuilder();
        JsonWriter json = new JsonWriter(sb);
        json.WriteObjectStart();
        json.WritePropertyName("name");
        json.Write("camera.setOptions");
        json.WritePropertyName("parameters");
        json.WriteObjectStart();
        json.WritePropertyName("options");
        json.WriteObjectStart();
        json.WritePropertyName("offDelay");
        json.Write(65535);
        json.WritePropertyName("fileFormat");
        json.WriteObjectStart();
        json.WritePropertyName("type");
        json.Write("jpeg");
        json.WritePropertyName("width");
        json.Write(5376);
        json.WritePropertyName("height");
        json.Write(2688);
        json.WriteObjectEnd();
        json.WriteObjectEnd();
        json.WriteObjectEnd();
        json.WriteObjectEnd();

        return sb.ToString();
    }

    private void AskTakePicture()
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
        json.WriteObjectEnd();
        json.WriteObjectEnd();

        return sb.ToString();
    }

    private void AskProgressStatus()
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

    private void AskDownloadPhoto()
    {
        mHTTP.ChangeCommand(mFileURL);
        mHTTP.Execute();
    }

    private void AskDeletePhoto()
    {
        mHTTP.ChangeCommand(HttpRequest.Commands.POST_C_EXECUTE);
        mHTTP.SetJSONData(ConstructDeletePhotoJSONString());
        mHTTP.Execute();
    }

    private string ConstructDeletePhotoJSONString()
    {
        StringBuilder sb = new StringBuilder();
        JsonWriter json = new JsonWriter(sb);
        json.WriteObjectStart();
        json.WritePropertyName("name");
        json.Write("camera.delete");
        json.WritePropertyName("parameters");
        json.WriteObjectStart();
        json.WritePropertyName("fileUrls");
        json.WriteArrayStart();
        json.Write(mFileURL);
        json.WriteArrayEnd();
        json.WriteObjectEnd();
        json.WriteObjectEnd();

        return sb.ToString();
    }
}
