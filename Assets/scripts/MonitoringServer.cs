using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class MonitoringServer : MonoBehaviour
{
    CameraData mCamData;
    BatteryManager mTabletBattery;
    static string PORT = "334";
    static string URL = "http://server.instant-sphere.com";


    private string GetLogURL()
    {
        return URL + ":" + PORT + "/api/logs";
    }

    private string GetHardwareURL()
    {
        return URL + ":" + PORT + "/api/hardware";
    }

    void Start()
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
	 * Coroutine sending log files and hardware values every [10 seconds]
	 **/
    IEnumerator SendRequest()
    {
        Logger logger = Logger.Instance;

        while (true)
        {
            string[] filenames = GetLogsFilesNames();

            foreach (string f in filenames)
            {
                // Creates a form containing the data to send
                WWWForm form = new WWWForm();
                form.AddBinaryData("logUploader", Encoding.UTF8.GetBytes(GetFileContent(f)), f, "text/plain");
                UnityWebRequest www = UnityWebRequest.Post(GetLogURL(), form);

                // Sends the request and yields until the send completes
                yield return www.SendWebRequest();

                if (www.isNetworkError || www.isHttpError)
                {
                    Debug.Log(www.error);
                }
                else
                {
                    File.Delete(Application.persistentDataPath + "/" + f);
                    Debug.Log("SENDING LOG FILE : " + f);
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
            yield return www2.SendWebRequest();

            if (www2.isNetworkError || www2.isHttpError)
            {
                Debug.Log(www2.error);
            }

            yield return new WaitForSeconds(10.0f);
        }
    }

    public static string GetFileContent(string filename)
    {
        StreamReader streamReader = new StreamReader(Application.persistentDataPath + "/" + filename);
        string res = streamReader.ReadToEnd();
        streamReader.Close();
        return res;
    }
}
