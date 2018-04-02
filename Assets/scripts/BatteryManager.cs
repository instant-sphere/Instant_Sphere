using UnityEngine;

/**
 * This class is used to get information about the tablet battery
 **/
public sealed class BatteryManager
{
    AndroidJavaObject mBatteryCtrl;    //native Java code object used to manipulate battery settings

    public BatteryManager()
    {
#if !UNITY_EDITOR
        AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject activity = jc.GetStatic<AndroidJavaObject>("currentActivity");
        mBatteryCtrl = new AndroidJavaObject("com.AEI.BornePhoto.Battery", activity);
#endif
    }

    /**
     * Return the current battery level or 0 in editor mode
     **/
    public int GetCurrentBatteryLevel()
    {
#if !UNITY_EDITOR
        return mBatteryCtrl.Call<int>("BatteryLevel");
#else
        return 0;
#endif
    }
}
