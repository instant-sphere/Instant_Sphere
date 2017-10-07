using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/**
 * Execute POST request to the camera HTTP server.
 * Return the camera response
 **/

public class HttpPost : MonoBehaviour
{
    private string mIPAdress;
    private string mCommand;
    private Dictionary<string, string> mHeader = new Dictionary<string, string>();
    private Dictionary<string, string> mData;

    public enum Commands {C_EXECUTE = 0, C_STATUS, STATE, CHECK_UPDATE};
    private string[] mCommandsValues = {"/osc/commands/execute", "/osc/commands/status", "/osc/state", "/osc/checkForUpdates"};

    void Start()
    {
        mHeader.Add("Content-Type", "application/json");
        mIPAdress = "192.168.1.1";
        ChangeCommand(Commands.C_EXECUTE);
        AddJSONData("name", "camera.startSession");
        Execute();
    }

    void ChangeCommand(Commands newCommand)
    {
        mCommand = mCommandsValues[(int)newCommand];
    }

    void AddJSONData(string name, string value)
    {
        if (mData == null)
            mData = new Dictionary<string, string>();
        mData.Add(name, value);
    }

    IEnumerator WaitForWWW(WWW www)
    {
        yield return www;

        string txt = "";
        if (string.IsNullOrEmpty(www.error))
            txt = www.text;  //text of success
        else
            txt = www.error;  //error
        Debug.Log("Response:" + txt);
    }

    void Execute()
    {
        try
        {
            // Construct POST string
            string postData = "{";
            foreach(KeyValuePair<string, string> KV in mData)
            {
                postData += "\"" + KV.Key + "\" : \"" + KV.Value + "\",";
            }
            postData = postData.TrimEnd(',');
            postData += "}";

            // Reset data
            mData.Clear();

            // Send POST request and start waiting for response
            byte[] pData = System.Text.Encoding.UTF8.GetBytes(postData.ToCharArray());
            WWW api = new WWW(mIPAdress + mCommand, pData, mHeader);
            StartCoroutine(WaitForWWW(api));
        }
        catch (UnityException ex)
        {
            Debug.Log(ex.Message);
        }
    }

}