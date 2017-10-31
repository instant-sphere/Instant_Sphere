using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class screens_controller : MonoBehaviour
{
    [SerializeField]
    List<Canvas> mScreens;

    [SerializeField]
    Text mCountDown;

    enum ScreensStates { WELCOME = 0, READY_TAKE_PHOTO, TAKING_PHOTO, DISPLAY_PHOTO, SHARE_PHOTO };
    ScreensStates mCurrentState = ScreensStates.WELCOME;

    enum InterfaceButtons { TAKE_PHOTO = 0, ABORT, RETRY, OK, BACK };
    bool[] mButtonsActivated = new bool[5];

    [SerializeField]
    osc_controller mOSCController;

    CounterDown mCounter = new CounterDown();

    private class CounterDown
    {
        bool mCountingDown = false;
        bool mCountDownEnded = false;

        public IEnumerator Count(int start, Action<int> update)
        {
            mCountingDown = true;
            for (int i = 3; i >= 0; i--)
            {
                update(i);
                yield return new WaitForSeconds(1.0f);
            }
            mCountDownEnded = true;
        }

        public bool IsCounting()
        {
            return mCountingDown;
        }

        public bool IsCounterFinished()
        {
            return mCountDownEnded;
        }
    }

    // Use this for initialization
    void Start()
    {
        UpdateScreen();
    }

    // Update is called once per frame
    void Update()
    {
        switch (mCurrentState)
        {
            case ScreensStates.WELCOME:
                if (Input.touchCount > 0 || Input.GetMouseButton(0))
                {
                    mCurrentState = ScreensStates.READY_TAKE_PHOTO;
                    UpdateScreen();
                }
                break;

            case ScreensStates.READY_TAKE_PHOTO:
                if (IsButtonDown(InterfaceButtons.TAKE_PHOTO))
                {
                    mCurrentState = ScreensStates.TAKING_PHOTO;
                    UpdateScreen();
                }
                break;

            case ScreensStates.TAKING_PHOTO:
                if (!mCounter.IsCounting())
                {
                    StartCoroutine(mCounter.Count(3, UpdateCountDownText));
                }
                else if(mCounter.IsCounterFinished())
                {
                    mCurrentState = ScreensStates.DISPLAY_PHOTO;
                    mCounter = new CounterDown();
                    UpdateScreen();
                }
                break;
            case ScreensStates.DISPLAY_PHOTO:
                if(IsButtonDown(InterfaceButtons.ABORT))
                {
                    mCurrentState = ScreensStates.WELCOME;
                }
                else if(IsButtonDown(InterfaceButtons.RETRY))
                {
                    mCurrentState = ScreensStates.READY_TAKE_PHOTO;
                }
                else if(IsButtonDown(InterfaceButtons.OK))
                {
                    mCurrentState = ScreensStates.SHARE_PHOTO;
                }
                UpdateScreen();
                break;
            case ScreensStates.SHARE_PHOTO:
                if(IsButtonDown(InterfaceButtons.BACK))
                {
                    mCurrentState = ScreensStates.DISPLAY_PHOTO;
                }
                break;
        }

        ResetButtons();
    }

    private void UpdateCountDownText(int v)
    {
        mCountDown.text = v.ToString();
    }

    private void UpdateScreen()
    {
        foreach (Canvas s in mScreens)
        {
            s.gameObject.SetActive(false);
        }

        mScreens[(int)mCurrentState].gameObject.SetActive(true);
    }

    private void ResetButtons()
    {
        for (int i = 0; i < mButtonsActivated.Length; ++i)
        {
            mButtonsActivated[i] = false;
        }
    }

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

    private void SetButtonDown(InterfaceButtons b)
    {
        mButtonsActivated[(int)b] = true;
    }

    private bool IsButtonDown(InterfaceButtons b)
    {
        return mButtonsActivated[(int)b];
    }
}
