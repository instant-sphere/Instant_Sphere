using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using LitJson;

/**
 * Execute POST or GET request to the camera HTTP server.
 * You have to add all JSON request parameters using AddJSONData() method.
 * The class has a static method JSONStringToDictionary() to convert server response into usable data structure.
 **/

public class HttpRequest : MonoBehaviour
{
    private string mIPAdress;   //server IP address
    private string mCommand;    //current API command
    private string mResponse;   //last received response
    private bool mReady;        //is last request response ready?
    private Dictionary<string, string> mHeader = new Dictionary<string, string>();  //POST headers
    private Dictionary<string, string> mData = new Dictionary<string, string>();    //JSON data for POST

    public enum Commands {POST_C_EXECUTE = 0, POST_C_STATUS, POST_STATE, POST_CHECK_UPDATE, GET_INFO};  //enumeration of all API commands
    private string[] mCommandsValues = {"/osc/commands/execute", "/osc/commands/status", "/osc/state", "/osc/checkForUpdates", "/osc/info"}; //string values for API commands

    /**
     * Convert a string containing valid JSON into a dictionary like structure.
     **/
    public static JsonData JSONStringToDictionary(string json)
    {
        JsonData data = JsonMapper.ToObject(json);
        return data;
    }

    /**
     * Unity start method used to setup object
     **/
    void Start()
    {
        mHeader.Add("Content-Type", "application/json");
        mIPAdress = "192.168.1.1";
        mReady = false;
        ChangeCommand(Commands.POST_C_EXECUTE);
    }

    /**
     * Use this to change the current API command target
     * ie: ChangeCommand(Commands.POST_STATE);
     **/
    public void ChangeCommand(Commands newCommand)
    {
        mCommand = mCommandsValues[(int)newCommand];
    }

    /**
     * Add a JSON key/value couple to next request.
     * If you don't add JSON next request will be a GET request, otherwise it will be a POST request.
     * So add dummy JSON if you want to do a POST request without parameter.
     **/
    public void AddJSONData(string name, string value)
    {
        mData.Add(name, value);
    }

    /**
     * Send the next request to the server
     * Use GetHTTPResponse() to retrieve the response
     **/
    public void Execute()
    {
        mReady = false;
        WWW api;
        try
        {
            // Construct POST string if needed
            if(mData.Count > 0)
            {
                // Send POST request and start waiting for response
                byte[] postData = System.Text.Encoding.UTF8.GetBytes(ConstructPOSTString().ToCharArray());
                api = new WWW(mIPAdress + mCommand, postData, mHeader);

                // Reset data
                mData.Clear();
            }
            else //GET request
            {
                api = new WWW(mIPAdress + mCommand);
            }
            
            StartCoroutine(WaitForWWW(api));
        }
        catch (UnityException ex)
        {
            Debug.Log(ex.Message);
        }
    }

    /**
     * Return the HTTP response of last request as string
     * Return null if the response isn't ready
     * A request should have been executed before calling this method
     **/
    public string GetHTTPResponse()
    {
        if (mReady)
        {
            mReady = false;
            return mResponse;
        }
        else
            return null;         
    }

    /**
     * Unity coroutine to wait for server response
     **/
    private IEnumerator WaitForWWW(WWW www)
    {
        yield return www;

        string txt = "";
        if (string.IsNullOrEmpty(www.error))
            txt = www.text;  //text of success
        else
            txt = www.error;  //error
        mResponse = txt;
        mReady = true;
    }

    /**
     * Construct the POST string from the key/value pairs
     **/
    private string ConstructPOSTString()
    {
        string postData = "{";
        foreach (KeyValuePair<string, string> KV in mData)
        {
            postData += "\"" + KV.Key + "\" : \"" + KV.Value + "\",";
        }
        postData = postData.TrimEnd(',');
        postData += "}";

        return postData;
    }

    /* DEBUG */
    private void Update()
    {
        string r = GetHTTPResponse();
        if(r != null)
        {
            JsonData d = JSONStringToDictionary(r);
            Debug.Log(d["fingerprint"]);
        }  
    }
    /* DEBUG */
}