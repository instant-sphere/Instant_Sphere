using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LitJson;

/**
 * Execute POST or GET request to the camera HTTP server.
 * You have to add all JSON request parameters using AddJSONData() method.
 * The class has a static method JSONStringToDictionary() to convert server response into usable data structure.
 **/

public sealed class HttpRequest
{
    string mIPAdress;   //server IP address
    string mCommand;    //current API command
    WWW mRequest;       //WWW object to send requests
    RequestType mType;  //GET or POST
    Dictionary<string, string> mHeader = new Dictionary<string, string>();  //POST headers
    Dictionary<string, string> mData = new Dictionary<string, string>();    //JSON data buffer for POST
    enum RequestType { GET, POST };
    List<KeyValuePair<string, RequestType>> mCommandsValues = new List<KeyValuePair<string, RequestType>>();

    public enum Commands { POST_C_EXECUTE = 0, POST_C_STATUS, POST_STATE, POST_CHECK_UPDATE, GET_INFO };  //enumeration of all API commands

    /**
     * Convert a string containing valid JSON into a dictionary like structure.
     **/
    public static JsonData JSONStringToDictionary(string json)
    {
        JsonData data = JsonMapper.ToObject(json);
        return data;
    }

    /**
     * Constructor
     **/
    public HttpRequest()
    {
        string[] tmpCommandString = { "/osc/commands/execute", "/osc/commands/status", "/osc/state", "/osc/checkForUpdates", "/osc/info" };
        RequestType[] tmpCommandType = { RequestType.POST, RequestType.POST, RequestType.POST, RequestType.POST, RequestType.GET };

        for(int i = 0; i < tmpCommandString.Length; ++i)
        {
            KeyValuePair<string, RequestType> tmp = new KeyValuePair<string, RequestType>(tmpCommandString[i], tmpCommandType[i]);
            mCommandsValues.Add(tmp);
        }

        mHeader.Add("Content-Type", "application/json");
        mIPAdress = "192.168.1.1";
    }

    /**
     * Use this to change the current API command target
     * ie: ChangeCommand(Commands.POST_STATE);
     **/
    public void ChangeCommand(Commands newCommand)
    {
        mCommand = mCommandsValues[(int)newCommand].Key;
        mType = mCommandsValues[(int)newCommand].Value;
    }

    /**
     * Add a JSON key/value couple to next request
     * Request should be of type POST
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
        try
        {
            // Construct POST string if needed
            if(mType == RequestType.POST)
            {
                // Send POST request and start waiting for response
                byte[] postData = System.Text.Encoding.UTF8.GetBytes(ConstructPOSTString().ToCharArray());
                mRequest = new WWW(mIPAdress + mCommand, postData, mHeader);

                // Reset data buffer
                mData.Clear();
            }
            else //GET request
            {
                mRequest = new WWW(mIPAdress + mCommand);
            }
        }
        catch (UnityException ex)
        {
            Debug.Log(ex.Message);
        }
    }

    /**
     * Return the HTTP response of last request made or error message on error
     * Return null if the response isn't ready
     * A request should have been executed before calling this method !
     **/
    public string GetHTTPResponse()
    {
        if (mRequest != null && mRequest.isDone)
        {
            string r = mRequest.error;
            if(r == null)
                r = mRequest.text;
            mRequest = null;
            return r;
        }
        else
            return null;         
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
}