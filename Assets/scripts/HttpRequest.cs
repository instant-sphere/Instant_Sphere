using UnityEngine;
using System.Collections.Generic;
using LitJson;
using System.Text.RegularExpressions;

/**
 * Execute POST or GET request to the camera HTTP server.
 * The class has a static method JSONStringToDictionary() to convert server response into usable data structure.
 **/

public sealed class HttpRequest
{
    string mIPAdress;   //server IP address
    string mCommand;    //current API command
    public WWW mRequest;       //WWW object to send requests
    RequestType mType;  //GET or POST
    bool mLastRequestSuccessful;
    Dictionary<string, string> mHeader = new Dictionary<string, string>();  //POST headers
    string mData = "";    //JSON data buffer for POST
    enum RequestType { GET, POST };
    List<KeyValuePair<string, RequestType>> mCommandsValues = new List<KeyValuePair<string, RequestType>>();

    public enum Commands { POST_C_EXECUTE = 0, POST_C_STATUS, POST_STATE, POST_CHECK_UPDATE, GET_INFO, GET_URI };  //enumeration of all API commands

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
        string[] tmpCommandString = { "/osc/commands/execute", "/osc/commands/status", "/osc/state", "/osc/checkForUpdates", "/osc/info", ""};
        RequestType[] tmpCommandType = { RequestType.POST, RequestType.POST, RequestType.POST, RequestType.POST, RequestType.GET, RequestType.GET };

        for(int i = 0; i < tmpCommandString.Length; ++i)
        {
            KeyValuePair<string, RequestType> tmp = new KeyValuePair<string, RequestType>(tmpCommandString[i], tmpCommandType[i]);
            mCommandsValues.Add(tmp);
        }

        mHeader.Add("Content-Type", "application/json");
        mIPAdress = "http://192.168.1.1";
    }

    /**
     * Use this to change the current API command target
     * ie: ChangeCommand(Commands.POST_STATE);
     * Or to set the URI to download a file
     * ie: ChangeCommand("image023.JPG");
     **/
    public void ChangeCommand(Commands newCommand)
    {
        mCommand = mCommandsValues[(int)newCommand].Key;
        mType = mCommandsValues[(int)newCommand].Value;
    }

    public void ChangeCommand(string URL)
    {
        ChangeCommand(Commands.GET_URI);
        string withoutIP = Regex.Match(URL, @"" + mIPAdress + "(.+)", RegexOptions.Singleline).Groups[1].Value;
        mCommand += withoutIP;
    }

    /**
     * Set JSON data for next request
     * Request should be of type POST
     **/
    public void SetJSONData(string json)
    {
        mData = json;
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
                byte[] postData = System.Text.Encoding.UTF8.GetBytes(mData.ToCharArray());
                mRequest = new WWW(mIPAdress + mCommand, postData, mHeader);

                // Reset data buffer
                mData = "";
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

    /*
     * Return true if a request has been made and its result is ready
     **/
    public bool IsTerminated()
    {
        return mRequest != null && mRequest.isDone;
    }

    public bool IsSuccessful()
    {
        return mLastRequestSuccessful;
    }

    /**
     * Return the HTTP response of last request made or error message on error
     * Return null if the response isn't ready
     * A request should have been executed before calling this method !
     **/
    public string GetHTTPResponse()
    {
        if (IsTerminated())
        {
            string r = mRequest.error;
            mLastRequestSuccessful = false;
            if(r == null)
            {
                r = mRequest.text;
                mLastRequestSuccessful = true;
            }
               
            return r;
        }
        else
            return null;         
    }

    public byte[] GetRawResponse()
    {
        if (IsTerminated())
        {
            return mRequest.bytes;
        }
        return null;
    }
}