using System;
using System.Collections;
using System.Collections.Generic;
using LitJson;
using UnityEngine;
using UnityEngine.Networking;
using Debug = UnityEngine.Debug;

/**
 * This class polls the camera and keeps data about the battery and error state
 **/
public class CameraData : MonoBehaviour
{
	struct CameraHardwareData
	{
		public float batteryLevel; // (0.0, 0.33, 0.67, or 1.0)
		public string batteryState; // "charging", "charged", "disconnect"
		public string cameraError;
	}

    CameraHardwareData mCameraData;

	/* Camera error codes associated to event flags */
	Dictionary<string, string> errorCodes = new Dictionary<string, string>();

	/**
	 * Initiates error codes according to the camera documentation
	 **/
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

    /**
     * Starts the coroutine and setup internal structure
     **/
	void Start()
	{
        InitErrorCodes();
		StartCoroutine(SendRequest());
	}


	/**
	 * Coroutine sending request every 30 sec and updating camera data
	 **/
	IEnumerator SendRequest()
	{
		while (true)
		{
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

            Debug.Log("CAMERA DATA : battery : " + mCameraData.batteryLevel + " battery state : " + mCameraData.batteryState + "camera error : " + mCameraData.cameraError);
			yield return new WaitForSeconds(30.0f);

		}
	}

    /**
     * Returns the camera battery level
     **/
	public float GetCameraBattery()
	{
		return mCameraData.batteryLevel;
	}

    /**
     * Returns the camera battery state
     **/
	public string GetCameraBatteryState()
	{
        return mCameraData.batteryState;
	}
}
