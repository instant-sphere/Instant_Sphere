using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/**
 * This class handle which application screen is being shown
 * It is also responsible for calling OSC controller methods
 **/
public sealed class screens_controller : MonoBehaviour
{
    //Unity components set in inspector
    public List<Canvas> mScreens;
    public Text mCountDown;
    public rotateCamera mCamera;
    public osc_controller mOSCController;
    public skybox_manager mSkyboxMng;

    //one state per screen
    enum ScreensStates { WELCOME = 0, READY_TAKE_PHOTO, TAKING_PHOTO, WAITING, DISPLAY_PHOTO, SHARE_PHOTO };
    ScreensStates mCurrentState;

    //interface buttons
    enum InterfaceButtons { TAKE_PHOTO = 0, ABORT, RETRY, OK, BACK };
    bool[] mButtonsActivated = new bool[5]; //buffer

    bool mIsOSCReady = false;

    //count down used when taking a photo
    CounterDown mCounter = new CounterDown();

    /* Use this for initialization */
    private void Start()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;  //device screen should never turn off
        mCurrentState = ScreensStates.WELCOME;          //start application on welcome screen
        mCamera.AutomaticRotation();                    //use automatic rotation of welcome photo
        UpdateScreen();
    }

    /* Update is called once per frame */
    private void Update()
    {
        //handle user interactions
        ManageStates();
        ResetButtons();
    }

    /**
     * Method to be used as callback to signal that photo is ready
     **/
    public void TriggerOSCReady()
    {
        mIsOSCReady = true;
    }

    /* Theses are public methods called when user presses the corresponding button */
    public void ButtonTakePhoto()
    {
        SetButtonDown(InterfaceButtons.TAKE_PHOTO);
    }

    public void ButtonAbort()
    {
        SetButtonDown(InterfaceButtons.ABORT);
    }

    public void ButtonRetry()
    {
        SetButtonDown(InterfaceButtons.RETRY);
    }

    public void ButtonOK()
    {
        SetButtonDown(InterfaceButtons.OK);
    }

    public void ButtonBack()
    {
        SetButtonDown(InterfaceButtons.BACK);
    }

    /**
     * Memorize that button b has been triggered
     **/
    private void SetButtonDown(InterfaceButtons b)
    {
        mButtonsActivated[(int)b] = true;
    }

    /**
     * Return true if button b has been pressed since last buffer reset
     **/
    private bool IsButtonDown(InterfaceButtons b)
    {
        return mButtonsActivated[(int)b];
    }

    /**
     * Reset buttons buffer
     **/
    private void ResetButtons()
    {
        for (int i = 0; i < mButtonsActivated.Length; ++i)
            mButtonsActivated[i] = false;
    }

    /**
     * Change counter text on screen and set it to v
     **/
    private void UpdateCountDownText(int v)
    {
        mCountDown.text = v.ToString();
    }

    /**
     * Update the shown canvas according to current state
     **/
    private void UpdateScreen()
    {
        foreach (Canvas s in mScreens)
            s.gameObject.SetActive(false);

        mScreens[(int)mCurrentState].gameObject.SetActive(true);
    }

    /**
     * Manage screens using internal state
     **/
    private void ManageStates()
    {
        switch (mCurrentState)
        {
            case ScreensStates.WELCOME:
                ManageWelcomeScreen();
                break;
            case ScreensStates.READY_TAKE_PHOTO:
                ManageReadyTakePhotoScreen();
                break;
            case ScreensStates.TAKING_PHOTO:
                ManageTakingPhotoScreen();
                break;
            case ScreensStates.WAITING:
                ManageWaitingScreen();
                break;
            case ScreensStates.DISPLAY_PHOTO:
                ManageDisplayScreen();
                break;
            case ScreensStates.SHARE_PHOTO:
                ManageShareScreen();
                break;
        }
    }

    /**
     * If user touch the welcome screen stop automatic rotation
     * and go to take a photo screen
     **/
    private void ManageWelcomeScreen()
    {
        if (Input.touchCount > 0 || Input.GetMouseButton(0))
        {
            mCurrentState = ScreensStates.READY_TAKE_PHOTO;
            mCamera.ManualRotation();
            UpdateScreen();
        }
    }

    /**
     * If user pushes the Take a Photo button go to screen taking photo
     **/
    private void ManageReadyTakePhotoScreen()
    {
        if (IsButtonDown(InterfaceButtons.TAKE_PHOTO))
        {
            mCurrentState = ScreensStates.TAKING_PHOTO;
            UpdateScreen();
        }
    }

    /**
     * Start the countdown if it's not
     * If the countdown is finished, ask camera to capture a photo and go to waiting screen
     **/
    private void ManageTakingPhotoScreen()
    {
        if (!mCounter.IsCounting())
        {
            StartCoroutine(mCounter.Count(3, UpdateCountDownText));
        }
        else if (mCounter.IsCounterFinished())
        {
            mIsOSCReady = false;
            mOSCController.StartCapture(TriggerOSCReady);
            mCurrentState = ScreensStates.WAITING;
            mCounter = new CounterDown();
            UpdateScreen();
        }
    }

    /**
     * Wait until the OSC controller signal that the photo is ready
     * Then retrieve the data, save them and go to display screen
     **/
    private void ManageWaitingScreen() //TODO wait for photo to be downloaded with some animation.
    {
        if (mIsOSCReady)
        {
            byte[] photoData = mOSCController.GetLatestData();
            mSkyboxMng.DefineNewSkybox(photoData);

            mCurrentState = ScreensStates.DISPLAY_PHOTO;
            UpdateScreen();
        }
    }

    /**
     * On the display screen go to the share screen when user presses the OK button
     * Go to back to take a photo if user presses the RETRY button
     * Go to welcome screen if user presses the ABORT button
     **/
    private void ManageDisplayScreen()
    {
        if (IsButtonDown(InterfaceButtons.ABORT))
        {
            mSkyboxMng.ResetSkybox();
            mCurrentState = ScreensStates.WELCOME;
            mCamera.AutomaticRotation();
        }
        else if (IsButtonDown(InterfaceButtons.RETRY))
        {
            mSkyboxMng.ResetSkybox(); //TODO set to preview
            mCurrentState = ScreensStates.READY_TAKE_PHOTO;
        }
        else if (IsButtonDown(InterfaceButtons.OK))
        {
            mCurrentState = ScreensStates.SHARE_PHOTO;
        }
        else
            return;
        UpdateScreen();
    }

    /**
     * On the share screen go back to display screen if user presses the BACK button
     **/
    private void ManageShareScreen()
    {
        if (IsButtonDown(InterfaceButtons.BACK))
        {
            mCurrentState = ScreensStates.DISPLAY_PHOTO;
            UpdateScreen();
        }
    }
}

/**
 * Class to handle counter down through Unity callback
 **/
public class CounterDown
{
    bool mCountingDown = false;
    bool mCountDownEnded = false;

    /**
     * Start counting from start to 0 second. Calls update(i) each time 
     * This is a coroutine
     **/
    public IEnumerator Count(int start, Action<int> update)
    {
        mCountingDown = true;
        for (int i = start; i >= 0; i--)
        {
            update(i);
            yield return new WaitForSeconds(1.0f);
        }
        mCountDownEnded = true;
    }

    /**
     * Returns true if counter has started
     **/
    public bool IsCounting()
    {
        return mCountingDown;
    }

    /**
     * Returns true if counter has reached 0
     **/
    public bool IsCounterFinished()
    {
        return mCountDownEnded;
    }
}