package com.AEI.BornePhoto;

import android.app.Activity;
import android.provider.Settings;

/**
 * Created by beaus on 01/04/2018.
 */

public class Secure_ID {
    String mAndroidID;

    public Secure_ID(Activity act)
    {
        mAndroidID = Settings.Secure.getString(act.getApplicationContext().getContentResolver(), Settings.Secure.ANDROID_ID);
    }

    public String SID()
    {
        return mAndroidID;
    }
}
