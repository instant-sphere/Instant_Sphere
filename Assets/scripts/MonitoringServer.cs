using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

/**
 * This class sends the logs and other hardware data to the server
 **/
public sealed class MonitoringServer : MonoBehaviour
{
    public CameraData mCamData;
    public Sharing mSharingServer;

    BatteryManager mTabletBattery;
    // static string URL = "https://server.instant-sphere.com";
    static string URL = "http://127.0.0.1:3333";
    

    /**
     * Returns the URL for the logs end point
     **/
    private string GetLogURL()
    {
        return URL + "/api/logs";
    }

    /**
     * Returns the URL for the hardware data end point
     **/
    private string GetHardwareURL()
    {
        return URL + "/api/hardware";
    }

    /**
     * Starts the coroutine that continuously sends data to the server
     **/
    private void Start()
    {
        mTabletBattery = new BatteryManager();
        StartCoroutine(SendRequest());
    }

    /**
	 * Returns the list of logs files names
	 **/
    private string[] GetLogsFilesNames()
    {
        DirectoryInfo dir = new DirectoryInfo(Application.persistentDataPath + "/");
        FileInfo[] filesInfos = dir.GetFiles();
        List<string> filesNames = new List<string>(filesInfos.Length);

        foreach (var f in filesInfos)
        {
            if (f.Extension == ".log")
            {
                filesNames.Add(f.Name);
            }
        }
        return filesNames.ToArray();
    }

    /**
	 * Coroutine sending log files and hardware values every 10 minutes
	 **/
    IEnumerator SendRequest()
    {
        Logger logger = Logger.Instance;

        while (true)
        {
            yield return new WaitForSeconds(300.0f);

            string[] filenames = GetLogsFilesNames();

            foreach (string f in filenames)
            {
                Debug.Log("Monitoring : treating file :" + f);
                // Creates a form containing the data to send
                WWWForm form = new WWWForm();
                form.AddBinaryData("logUploader", Encoding.UTF8.GetBytes(GetFileContent(f)), f, "text/plain");

                UnityWebRequest www = UnityWebRequest.Post(GetLogURL(), form);
                www.SetRequestHeader("x-access-token", mSharingServer.GetToken());

                // Sends the request and yields until the send completes
                yield return www.SendWebRequest();

                if (www.isNetworkError || www.isHttpError)
                {
                    Debug.Log("Monitoring server error :" + www.downloadHandler.text);
                }
                else
                {
                    Debug.Log("Monitoring has send: " + f);
                    File.Delete(Application.persistentDataPath + "/" + f);
                }
            }

            StringBuilder sb = new StringBuilder();
            JsonWriter json = new JsonWriter(sb);
            json.WriteObjectStart();
            json.WritePropertyName("timestamp");
            json.Write(DateTime.Now.ToString());
            json.WritePropertyName("cameraBattery");
            json.Write(mCamData.GetCameraBattery());
            json.WritePropertyName("tabletBattery");
            json.Write(mTabletBattery.GetCurrentBatteryLevel());
            json.WriteObjectEnd();

            WWWForm form2 = new WWWForm();
            form2.AddField("data", sb.ToString());

            UnityWebRequest www2 = UnityWebRequest.Post(GetHardwareURL(), form2);
            www2.SetRequestHeader("x-access-token", mSharingServer.GetToken());
            yield return www2.SendWebRequest();

            if (www2.isNetworkError || www2.isHttpError)
            {
                Debug.Log(www2.error);
            }
        }
    }

    /**
     * Returns the file content of the given file name
     **/
    public static string GetFileContent(string filename)
    {
        StreamReader streamReader = new StreamReader(Application.persistentDataPath + "/" + filename);
        string res = streamReader.ReadToEnd();
        streamReader.Close();
        return res;
    }
}
