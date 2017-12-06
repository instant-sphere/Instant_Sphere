package com.AEI.BornePhoto;

import android.annotation.SuppressLint;
import android.app.Activity;
import android.content.Context;
import android.net.wifi.WifiConfiguration;
import android.net.wifi.WifiManager;

import java.util.List;
import java.util.logging.Logger;

/**
 * Created by beaus on 03/12/2017.
 */

public class WifiController {
    WifiManager mWifiCtrl;

    public WifiController(Activity act)
    {
        mWifiCtrl = (WifiManager) act.getApplicationContext().getSystemService(Context.WIFI_SERVICE);
    }

    @SuppressLint("MissingPermission")
    public String GetCurrentSSID()
    {
        return mWifiCtrl.getConnectionInfo().getSSID();
    }

    public boolean IsWifiON()
    {
        return mWifiCtrl.isWifiEnabled();
    }

    @SuppressLint("MissingPermission")
    public void EnableWifi()
    {
        mWifiCtrl.setWifiEnabled(true);
    }

    @SuppressLint("MissingPermission")
    public void DisableWifi()
    {
        mWifiCtrl.setWifiEnabled(false);
    }

    @SuppressLint("MissingPermission")
    public boolean ConnectToSSID(String SSID)
    {
        List<WifiConfiguration> net = mWifiCtrl.getConfiguredNetworks();

        if(net == null)
            return false;

        for (WifiConfiguration n:net)
        {
            if(n.SSID.equals("\"" + SSID + "\"")) {
                mWifiCtrl.enableNetwork(n.networkId, true);
                return true;
            }
        }
        return false;
    }
}
