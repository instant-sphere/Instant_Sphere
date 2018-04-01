using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using LitJson;
using UnityEngine;
using UnityEngine.Networking;
using Debug = UnityEngine.Debug;

public class CameraData : MonoBehaviour
{
	struct camera_data
	{
		public float batteryLevel; // (0.0, 0.33, 0.67, or 1.0)
		public string batteryState; // "charging", "charged", "disconnect"
		public string cameraError;
	}

    camera_data mCameraData;

	/* Camera error codes associated to event flags */
	Dictionary<string, string> errorCodes = new Dictionary<string, string>();

	/*
	 * Initiates error codes according to the camera documentation
	 */
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
	}

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
			/*UnityWebRequest wwwInfo = new UnityWebRequest("192.168.1.1/osc/info");
			wwwInfo.downloadHandler = new DownloadHandlerBuffer();

			// Sends the request and yields until the send completes
			yield return wwwInfo.SendWebRequest();

			if (wwwInfo.isNetworkError || wwwInfo.isHttpError)
			{
				Debug.Log(wwwInfo.error);
			}
			else
			{
                JsonData json = HttpRequest.JSONStringToDictionary(wwwInfo.downloadHandler.text);
			}*/

			UnityWebRequest wwwState = UnityWebRequest.Post("192.168.1.1/osc/state", "");
            wwwState.downloadHandler = new DownloadHandlerBuffer();
			yield return wwwState.SendWebRequest();

			if (wwwState.isNetworkError || wwwState.isHttpError)
			{
				Debug.Log(wwwState.error);
			}
			else
			{
                JsonData json = HttpRequest.JSONStringToDictionary(wwwState.downloadHandler.text);
                mCameraData.batteryLevel = float.Parse(json["state"]["batteryLevel"].ToString());
                mCameraData.batteryState = json["state"]["_batteryState"].ToString();
                try
                {
                    mCameraData.cameraError = json["state"]["_cameraError"].ToString();
                }
                catch(Exception e)
                { }
			}

            Debug.Log("CAMERA DATA : " + mCameraData.batteryLevel + mCameraData.batteryState + mCameraData.cameraError);
			// Suspends the coroutine execution for the given amount of seconds using scaled time.
			yield return new WaitForSeconds(10.0f);

		}
	}

	public float GetCameraBattery()
	{
		return mCameraData.batteryLevel;
	}

	public string GetCameraBatteryState()
	{
        return mCameraData.batteryState;
	}
}
