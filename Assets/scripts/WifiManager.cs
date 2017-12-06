using UnityEngine;

public class WifiManager {
    AndroidJavaObject mWifiCtrl;    //native Java code object used to manipulate Wifi settings
    string mSSID;                   //SSID of camera network
    const float mWifiTimeout = 10;  //10 sec to wait wifi turning ON

    public WifiManager()
    {
#if !UNITY_EDITOR
        AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject activity = jc.GetStatic<AndroidJavaObject>("currentActivity");
        mWifiCtrl = new AndroidJavaObject("com.AEI.BornePhoto.WifiController", activity);
#endif
    }

	public bool WaitForWifi()
    {
#if !UNITY_EDITOR
        float t = Time.realtimeSinceStartup;

        if (!mWifiCtrl.Call<bool>("IsWifiON"))
            mWifiCtrl.Call("EnableWifi");

        while (!mWifiCtrl.Call<bool>("IsWifiON"))
        {
            Debug.Log("Waiting Wifi...");
            if(Time.realtimeSinceStartup - t > mWifiTimeout)
            {
                Debug.Log("Can't enable wifi !");
                return false;
            }
        }
#endif
        return true;
    }

    public void SaveAndShutdownWifi()
    {
#if !UNITY_EDITOR
        mSSID = mWifiCtrl.Call<string>("GetCurrentSSID");
        mWifiCtrl.Call("DisableWifi");
#endif
    }

    public bool RestoreWifi()
    {
#if !UNITY_EDITOR
        if(WaitForWifi())
            return mWifiCtrl.Call<bool>("ConnectToSSID", mSSID);
        return false;
#else
        return true;
#endif
    }
}
