package com.AEI.BornePhoto;

import android.app.Activity;
import android.content.Intent;
import android.os.Bundle;
import com.unity3d.player.UnityPlayerActivity;
import android.os.Bundle;
import android.util.Log;

public class OverrideUnityPlayer extends UnityPlayerActivity {
    protected void onCreate(Bundle savedInstanceState) {
        // call UnityPlayerActivity.onCreate()
        super.onCreate(savedInstanceState);
        startLockTask();
    }
}