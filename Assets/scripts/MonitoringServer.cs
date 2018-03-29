using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Debug = UnityEngine.Debug;

public class MonitoringServer : MonoBehaviour
{
    // Local VM conf
    //	private string ip = "127.0.0.1";
    //	private string port = "2222";

    static string PORT = "334";

    public string GetURL()
    {
        return "server.instant-sphere.com:" + PORT;
    }

    void Start()
    {
        StartCoroutine(SendRequest());
    }

    /*
	 * Returns the list of logs files names
	 */
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

    /*
	 * Concatenates logs content and returns the result as a JSON array
	 */
    /*private string GetLogs()
    {
        string[] filenames = GetLogsFilesNames();
        string res = "[";
        int i = 1;
        Logger logger = Logger.Instance;

        foreach (var f in filenames)
        {
            if (!string.IsNullOrEmpty(logger.GetCurrentFileName()) && f != ".log")
            {
                string currentFile = logger.GetCurrentFileName() + ".log";

                if (currentFile != f)
                {
                    res += GetFileContent(f);
                    res += ", ";
                }
            }
            i++;
        }

        // Removes last comma
        if (res.Length > 1)
        {
            res = res.Substring(0, res.Length - 2);
        }

        res += " ]";

        return res;
    }*/

    /**
	 * Coroutine sending log files every [10 seconds]
	 */
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
                /*form.AddField("cameraInfo", CameraData.GetCameraInfo(), Encoding.UTF8);
                form.AddField("cameraState", CameraData.GetCameraState(), Encoding.UTF8);*/
                UnityWebRequest www = UnityWebRequest.Post(GetURL(), form);

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
            // Suspends the coroutine execution for the given amount of seconds using scaled time.
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
