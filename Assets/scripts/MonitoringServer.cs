using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.Networking;

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

	// Coroutine sending request every minute
	IEnumerator SendRequest()
	{
		while (true)
		{
			// Creates a form containing the data sent
			WWWForm form = new WWWForm();
			form.AddField("data", "coucou");

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

}
