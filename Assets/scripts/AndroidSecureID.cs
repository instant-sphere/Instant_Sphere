﻿using UnityEngine;

/**
 * This class is used to get the Android unique ID
 **/
public class AndroidSecureID
{
    AndroidJavaObject mAndroidSID;    //native Java code object used to manipulate battery settings

    public AndroidSecureID()
    {
#if !UNITY_EDITOR
        AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject activity = jc.GetStatic<AndroidJavaObject>("currentActivity");
        mAndroidSID = new AndroidJavaObject("com.AEI.BornePhoto.Secure_ID", activity);
#endif
    }

    /**
     * Return the Android SID
     **/
    public string GetSID()
    {
#if !UNITY_EDITOR
        return mAndroidSID.Call<string>("SID");
#else
        return "mooc_SID";
#endif
    }
}
