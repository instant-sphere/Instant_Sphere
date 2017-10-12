using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;

public class osc_controller : MonoBehaviour {

    
    private HttpRequest mHTTP = new HttpRequest();
    private JsonData mCameraInfo;

	// Use this for initialization
	void Start ()
    {
        AskCameraInfo();
	}
	
	// Update is called once per frame
	void Update ()
    {
        string s = mHTTP.GetHTTPResponse();
		if(s != null)
        {
            Debug.Log(s);
            try
            {
                mCameraInfo = HttpRequest.JSONStringToDictionary(s);
            }
            catch(JsonException)
            {
                Debug.Log(s); //TODO handle connection error
            }
           
            Debug.Log(mCameraInfo["manufacturer"]);
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
        mHTTP.AddJSONData("name", "camera.startSession");
        mHTTP.Execute();
    }

    private void AskTakePicture()
    {
        mHTTP.ChangeCommand(HttpRequest.Commands.POST_C_EXECUTE);
        mHTTP.AddJSONData("name", "camera.takePicture");
        mHTTP.Execute();
    }

    public byte[] TakePhoto()
    {
        byte[] img = { 1, 2, 3 };



        return img;
    }
}
