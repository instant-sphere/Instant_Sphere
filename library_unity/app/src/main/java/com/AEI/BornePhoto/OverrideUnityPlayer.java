package com.AEI.BornePhoto;

import android.os.Bundle;
import android.view.KeyEvent;

import com.unity3d.player.UnityPlayerActivity;

/**
 * Created by jbeaussart on 28/11/2017.
 */

public class OverrideUnityPlayer extends UnityPlayerActivity {
    protected void onCreate(Bundle savedInstanceState) {
        // call UnityPlayerActivity.onCreate()
        super.onCreate(savedInstanceState);
        startLockTask();
    }

    @Override
    public boolean dispatchKeyEvent(KeyEvent event) {
        int action = event.getAction();
        int keyCode = event.getKeyCode();
        // just violently kill application process when pressing volume buttons
        switch (keyCode) {
            case KeyEvent.KEYCODE_VOLUME_UP:
                if (action == KeyEvent.ACTION_DOWN) {
                    android.os.Process.killProcess(android.os.Process.myPid());
                }
                return true;
            case KeyEvent.KEYCODE_VOLUME_DOWN:
                if (action == KeyEvent.ACTION_DOWN) {
                    android.os.Process.killProcess(android.os.Process.myPid());
                }
                return true;
            default:
                return super.dispatchKeyEvent(event);
        }
    }
}