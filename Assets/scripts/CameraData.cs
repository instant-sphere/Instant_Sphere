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

public class CameraData : MonoBehaviour
{

	/*Son identifiant aléatoire (ID) généré au premier lancement de l'application
	La version de l'application, du firmware camera
	Le niveau de charge de la batterie de la tablette
	Le niveau de charge de la batterie de la caméra
	La présence (ou non) de l'alimentation secteur
	Les coordonnées GPS de la borne
	Le nombre de prises de vue les 30 dernières minutes
	Le nombre de partage sur chaque réseau social les 15 dernières minutes
	Le statut et un code d'erreur correspondant (timeout caméra, ...)
	*/

	struct camera_data
	{
		public float batteryLevel; // (0.0, 0.33, 0.67, or 1.0)
		public string batteryState; // "charging", "charged", "disconnect"

	}

	private camera_data mCameraData;
	
/*	/* Camera error codes associated to event flags #1#
	Dictionary<string, string> errorCodes = new Dictionary<string, string>();

	/*
	 * Initiates error codes according to the camera documentation
	 #1#
	private void InitErrorCodes()
	{
		string[] flags =
		{
			"0x00000001", "0x00000002", "0x00000004", "0x00000008", "0x00000010", "0x00000100", "0x00400000",
			"0x01000000", "0x02000000", "0x04000000", "0x08000000", "0x10000000", "0x20000000", "0x40000000",
			"0x80000000"
		};

		string[] codes =
		{
			"NO_MEMORY", "WRITING_DATA", "FILE_NUMBER_OVER", "NO_DATE_SETTING", "COMPASS_CALIBRATION",
			"CARD_DETECT_FAIL", "CAPTURE_HW_FAILED", "CANT_USE_THIS_CARD", "FORMAT_INTERNAL_MEM", "FORMAT_CARD",
			"INTERNAL_MEM_ACCESS_FAIL", "CARD_ACCESS_FAIL", "UNEXPECTED_ERROR", "BATTERY_CHARGE_FAIL",
			"HIGH_TEMPERATURE"
		};

		for (var i = 0; i < flags.Length; i++)
		{
			errorCodes.Add(flags[i], codes[i]);
		}
	}*/
	
	void Start()
	{	
		StartCoroutine(SendRequest());
	}


	/**
	 * Coroutine sending request every [10 seconds]
	 */
	IEnumerator SendRequest()
	{
		while (true)
		{
			UnityWebRequest wwwInfo = new UnityWebRequest("http://192.168.1.1/osc/info");
			// Sends the request and yields until the send completes
			yield return wwwInfo.SendWebRequest();
			
			if (wwwInfo.isNetworkError || wwwInfo.isHttpError)
			{
				Debug.Log(wwwInfo.error);
			}
			else
			{
				Debug.Log("Sending report to server");
			}

			string info = wwwInfo.downloadHandler.text;
			Debug.Log("CAMERA INFO : " + info);
			
			
			
			UnityWebRequest wwwState = UnityWebRequest.Post("http://192.168.1.1/osc/state", "");
			yield return wwwState.SendWebRequest();
			
			if (wwwState.isNetworkError || wwwState.isHttpError)
			{
				Debug.Log(wwwState.error);
			}
			else
			{
				Debug.Log("Sending report to server");
			}
			string state = wwwState.downloadHandler.text;
			
			// Suspends the coroutine execution for the given amount of seconds using scaled time.
			yield return new WaitForSeconds(10.0f);
			
		
	
		}
	}

}
