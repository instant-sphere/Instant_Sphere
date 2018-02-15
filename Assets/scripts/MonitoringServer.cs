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
	private string ip = "127.0.0.1";
	private string port = "2222";

	public string GetURL()
	{
		return "http://" + ip + ":" + port + "/";
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
		string[] filesNames = new string[filesInfos.Length];

		int i = 0;
		foreach (var f in filesInfos)
		{
			filesNames[i] = f.Name;
			i++;
		}
		
		Debug.Log("directory : " + Application.persistentDataPath + "/");

		return filesNames;
	}

	/*
	 * Concatenates logs content and returns the result as a JSON array
	 */
	private string GetLogs()
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
			Debug.Log("res : " + res);
			res = res.Substring(0, res.Length - 2);	
		}
		
		res += " ]";

		return res;
	}

	/**
	 * Coroutine sending request every [10 seconds]
	 */
	IEnumerator SendRequest()
	{
		while (true)
		{
//			Debug.Log("Camille est velue : " + GetLogs());
			
			// Creates a form containing the data sent
			WWWForm form = new WWWForm();
			form.AddField("data", GetLogs(), Encoding.UTF8);

			Debug.Log(GetURL());
			UnityWebRequest www = UnityWebRequest.Post(GetURL(), form);
			
			// Sends the request and yields until the send completes
			yield return www.Send();
			
			// Suspends the coroutine execution for the given amount of seconds using scaled time.
			yield return new WaitForSeconds(10.0f);

			if (www.isNetworkError || www.isHttpError)
			{
				Debug.Log(www.error);
			}
			else
			{
				Debug.Log("Sending report to server");
			}	
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
