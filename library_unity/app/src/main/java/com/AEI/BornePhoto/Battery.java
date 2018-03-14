package com.AEI.BornePhoto;

import android.app.Activity;
import android.content.Context;
import android.os.BatteryManager;

/**
 * Created by beaus on 14/03/2018.
 */

public class Battery {

    BatteryManager mBM;
    public Battery(Activity act)
    {
        mBM = (BatteryManager)act.getApplicationContext().getSystemService(Context.BATTERY_SERVICE);
    }

    int BatteryLevel()
    {
        return mBM.getIntProperty(BatteryManager.BATTERY_PROPERTY_CAPACITY);
    }
}
