using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using LitJson;
using UnityEngine.Networking;

/**
 * Execute POST or GET requests on the camera HTTP server.
 * The class has a static method JSONStringToDictionary() to convert server response into usable data structure.
 **/
public sealed class HttpRequest
{
    string mIPAdress;       //server IP address
    string mCommand;        //current API command
    int mTimeout;           //connection timeout
    UnityWebRequestAsyncOperation mWebRequest;  //object representing current connection for regular HTTP requests
    public streamingRequest mStreamRequest;
    RequestType mType;      //GET or POST type
    bool mIsStream;         //is current request a streaming request ?
    string mData = "";      //JSON data buffer for POST requests

    enum RequestType { GET, POST };
    List<KeyValuePair<string, RequestType>> mCommandsValues = new List<KeyValuePair<string, RequestType>>();    //hold API command string and request type, this list is indexed by 'Commands' enum

    public enum Commands { POST_C_EXECUTE = 0, POST_C_STATUS, POST_STATE, POST_CHECK_UPDATE, GET_INFO, GET_URL };  //enumeration of all API commands

    /**
     * Static method
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
        string[] tmpCommandString = { "/osc/commands/execute", "/osc/commands/status", "/osc/state", "/osc/checkForUpdates", "/osc/info", "" };
        RequestType[] tmpCommandType = { RequestType.POST, RequestType.POST, RequestType.POST, RequestType.POST, RequestType.GET, RequestType.GET };

        for (int i = 0; i < tmpCommandString.Length; ++i)
        {
            KeyValuePair<string, RequestType> tmp = new KeyValuePair<string, RequestType>(tmpCommandString[i], tmpCommandType[i]);
            mCommandsValues.Add(tmp);
        }

        mIPAdress = "http://192.168.1.1";
        mTimeout = 10;
        mIsStream = false;
    }

    /**
     * Use this to change the current API command target
     * ie: ChangeCommand(Commands.POST_STATE);
     * Or to set the URI to download a file
     * ie: ChangeCommand("URL");
     **/
    public void ChangeCommand(Commands newCommand)
    {
        mCommand = mCommandsValues[(int)newCommand].Key;
        mType = mCommandsValues[(int)newCommand].Value;
    }

    public void ChangeCommand(string URL)
    {
        ChangeCommand(Commands.GET_URL);
        string withoutIP = Regex.Match(URL, @"" + mIPAdress + "(.+)", RegexOptions.Singleline).Groups[1].Value;
        mCommand += withoutIP;
    }

    /**
     * Signal that next request is a streaming request
     **/
    public void NextRequestIsStream()
    {
        mIsStream = true;
    }

    /**
     * Cancel current streaming by aborting connection
     **/
    public void CloseStreaming()
    {
        if(mStreamRequest != null)
            mStreamRequest.Abort(); //close connection and terminate thread
        mStreamRequest = null;
        mIsStream = false;
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
        if(mWebRequest != null)
            mWebRequest.webRequest.Dispose();
        mWebRequest = null;
        if(mStreamRequest != null)
            mStreamRequest.Abort();
        mStreamRequest = null;

        try
        {
            byte[] postData = System.Text.Encoding.UTF8.GetBytes(mData.ToCharArray());

            if (mIsStream)  // Streaming request
            {
                mStreamRequest = new streamingRequest(mIPAdress + mCommand, mData);
                Debug.Log("Streaming:" + mIPAdress + mCommand + " : " + mData);
            }
            else
            {
                DownloadHandlerBuffer dlHandler = new DownloadHandlerBuffer();
                if (mType == RequestType.POST) //POST request
                {
                    UploadHandler upHandler = new UploadHandlerRaw(postData);
                    UnityWebRequest www = new UnityWebRequest(mIPAdress + mCommand, UnityWebRequest.kHttpVerbPOST, dlHandler, upHandler);
                    www.useHttpContinue = false;
                    www.timeout = mTimeout;
                    www.SetRequestHeader("Content-Type", "application/json");
                    mWebRequest = www.SendWebRequest();

                    Debug.Log("POST:" + mIPAdress + mCommand + ":" + mData);

                    mData = ""; // Reset data buffer
                }
                else //GET request
                {
                    UnityWebRequest www = new UnityWebRequest(mIPAdress + mCommand);
                    www.downloadHandler = dlHandler;
                    www.useHttpContinue = false;
                    www.timeout = mTimeout * 10;    //allowing more time to download files
                    mWebRequest = www.SendWebRequest();

                    Debug.Log("GET:" + mIPAdress + mCommand);
                }
            }
        }
        catch (UnityException ex)
        {
            Debug.Log(ex.Message);
        }
    }

    /**
     * Return true if a request has been made and its result is ready
     **/
    public bool IsTerminated()
    {
        return mWebRequest != null && mWebRequest.isDone;
    }

    /**
     * Return true if last terminated request was successful
     **/
    public bool IsSuccessful()
    {
        return mWebRequest != null && !mWebRequest.webRequest.isHttpError && !mWebRequest.webRequest.isNetworkError;
    }

    /**
     * Return the HTTP response of last request made as string or error message on error
     * Return null if the response isn't ready
     * A request should have been executed before calling this method !
     **/
    public string GetHTTPResponse()
    {
        if (IsTerminated())
        {
            string r = mWebRequest.webRequest.error;
            if (r == null)
            {
                r = mWebRequest.webRequest.downloadHandler.text;
            }
            return r;
        }
        else
            return null;
    }

    /**
     * Return the raw HTTP response as byte array
     * Useful for binary data
     * Return null if the response isn't ready or if there is an error
     * A request should have been executed before calling this method !
     **/
    public byte[] GetRawResponse()
    {
        if (IsTerminated() && IsSuccessful())
        {
            return mWebRequest.webRequest.downloadHandler.data;
        }
        return null;
    }
}