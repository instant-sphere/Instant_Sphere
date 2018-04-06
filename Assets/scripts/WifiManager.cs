using UnityEngine;

/**
 * This class is used to control wifi
 **/
public sealed class WifiManager
{
    AndroidJavaObject mWifiCtrl;    //native Java code object used to manipulate Wifi settings
    string mSSID;                   //SSID of camera network
    const float mWifiTimeout = 10;  //10 sec to wait for wifi turning ON

    public WifiManager()
    {
#if !UNITY_EDITOR
        AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject activity = jc.GetStatic<AndroidJavaObject>("currentActivity");
        mWifiCtrl = new AndroidJavaObject("com.AEI.BornePhoto.WifiController", activity);
#endif
    }

    /**
     * Enables wifi and waits until it is up or timeout expires
     * Returns true if wifi is enable and false otherwise
     * Always returns true in editor mode
     **/
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
                Debug.Log("Can't activate wifi !");
                return false;
            }
        }
#endif
        return true;
    }

    /**
     * Saves the current SSID and disables wifi
     **/
    public void SaveAndShutdownWifi()
    {
#if !UNITY_EDITOR
        mSSID = mWifiCtrl.Call<string>("GetCurrentSSID");
        mWifiCtrl.Call("DisableWifi");
#endif
    }

    /**
     * Re-enables wifi and tries to connect to the saved SSID
     * Returns true if the connection was successful, false otherwise
     * Always returns true in editor mode
     **/
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
