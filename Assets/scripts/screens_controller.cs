using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class screens_controller : MonoBehaviour
{
    /* Unity components set in inspector */
    public List<Canvas> mScreens;
    public Text mCountDown;
    public rotateCamera mCamera;
    public osc_controller mOSCController;

    /* One state per screen */
    enum ScreensStates { WELCOME = 0, READY_TAKE_PHOTO, TAKING_PHOTO, DISPLAY_PHOTO, SHARE_PHOTO };
    ScreensStates mCurrentState;

    /* Interface buttons */
    enum InterfaceButtons { TAKE_PHOTO = 0, ABORT, RETRY, OK, BACK };
    bool[] mButtonsActivated = new bool[5];

    /* Count down used when taking a photo */
    CounterDown mCounter = new CounterDown();

    /* Nested class to handle counter down */
    private class CounterDown
    {
        bool mCountingDown = false;
        bool mCountDownEnded = false;

        /* Start counting from start to 0 second. Calls update(i) each time 
         This is a coroutine */
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

        /* Returns true if counter has started */
        public bool IsCounting()
        {
            return mCountingDown;
        }

        /* Returns true if counter has reached 0 */
        public bool IsCounterFinished()
        {
            return mCountDownEnded;
        }
    }


    /* Use this for initialization */
    void Start()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        mCurrentState = ScreensStates.WELCOME;
        mCamera.AutomaticRotation();
        UpdateScreen();
    }

    /* Update is called once per frame */
    void Update()
    {
        /* Handle user interactions */
        ManageStates();
        ResetButtons();
    }

    /* Manages screens using internal state */
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
            case ScreensStates.DISPLAY_PHOTO:
                ManageDisplayScreen();
                break;
            case ScreensStates.SHARE_PHOTO:
                ManageShareScreen();
                break;
        }
    }

    private void ManageWelcomeScreen()
    {
        if (Input.touchCount > 0 || Input.GetMouseButton(0))
        {
            mCurrentState = ScreensStates.READY_TAKE_PHOTO;
            mCamera.ManualRotation();
            UpdateScreen();
        }
    }

    private void ManageReadyTakePhotoScreen()
    {
        if (IsButtonDown(InterfaceButtons.TAKE_PHOTO))
        {
            mCurrentState = ScreensStates.TAKING_PHOTO;
            UpdateScreen();
        }
    }

    private void ManageTakingPhotoScreen()
    {
        if (!mCounter.IsCounting())
        {
            StartCoroutine(mCounter.Count(3, UpdateCountDownText));
        }
        else if (mCounter.IsCounterFinished())
        {
            mCurrentState = ScreensStates.DISPLAY_PHOTO;
            mCounter = new CounterDown();
            UpdateScreen();
        }
    }

    private void ManageDisplayScreen()
    {
        if (IsButtonDown(InterfaceButtons.ABORT))
        {
            mCurrentState = ScreensStates.WELCOME;
            mCamera.AutomaticRotation();
        }
        else if (IsButtonDown(InterfaceButtons.RETRY))
        {
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

    private void ManageShareScreen()
    {
        if (IsButtonDown(InterfaceButtons.BACK))
        {
            mCurrentState = ScreensStates.DISPLAY_PHOTO;
            UpdateScreen();
        }
    }

    /* Changes counter text on screen and set it to v */
    private void UpdateCountDownText(int v)
    {
        mCountDown.text = v.ToString();
    }

    /* Flips canvas according to current state */
    private void UpdateScreen()
    {
        foreach (Canvas s in mScreens)
            s.gameObject.SetActive(false);

        mScreens[(int)mCurrentState].gameObject.SetActive(true);
    }

    /* Reset buttons buffer */
    private void ResetButtons()
    {
        for (int i = 0; i < mButtonsActivated.Length; ++i)
            mButtonsActivated[i] = false;
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

    /* Memorizes that button b has been triggered */
    private void SetButtonDown(InterfaceButtons b)
    {
        mButtonsActivated[(int)b] = true;
    }

    /* Return true if button b has been pressed since last buffer reset */
    private bool IsButtonDown(InterfaceButtons b)
    {
        return mButtonsActivated[(int)b];
    }
}
