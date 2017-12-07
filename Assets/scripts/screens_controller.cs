using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
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
    public Image mCountDown;
    public rotateCamera mCamera;
    public osc_controller mOSCController;
    public skybox_manager mSkyboxMng;

    //one state per screen
    enum ScreensStates { WELCOME = 0, READY_TAKE_PHOTO, TAKING_PHOTO, WAITING, DISPLAY_PHOTO, SHARE_PHOTO, ERROR };
    ScreensStates mCurrentState;

    //interface buttons
    enum InterfaceButtons { TAKE_PHOTO = 0, ABORT, RETRY, OK, BACK, SHARE_FB };
    bool[] mButtonsActivated = new bool[6]; //buffer

    bool mIsOSCReady = false;
    facebook mFB;
    WifiManager mWifi;
    byte[] mFullResolutionImage;

    //count down used when taking a photo
    CounterDown mCounter = new CounterDown();

    //Logs
    LogSD log = new LogSD();
    DateTime now;
    String now_str;

    /* Use this for initialization */
    private void Start()
    {
        mWifi = new WifiManager();
        if (!mWifi.WaitForWifi())                       //ensure that wifi is ON when app starts or quit
        {
            Application.Quit();
        }
        mFB = new facebook();
        Screen.sleepTimeout = SleepTimeout.NeverSleep;  //device screen should never turn off
        mCurrentState = ScreensStates.WELCOME;          //start application on welcome screen
        mCamera.AutomaticRotation();                    //use automatic rotation of welcome photo
        UpdateScreen();
    }

    /* Update is called once per frame */
    private void Update()
    {
        if (!mOSCController.IsCameraOK())   //go to error state and stay inside
        {
            // For logs
            now = System.DateTime.Now;
            now_str = now.ToString("MM-dd-yyyy_hh.ss.mm");
            log.new_date();
            log.WriteFile(log.file_date_str, "[\n\t{\"event\": \"timeout\", \"time\": \""+now_str+"\"}\n]" );

            mCurrentState = ScreensStates.ERROR;
            UpdateScreen();
        }
        else
        {
            //handle user interactions
            ManageStates();
            ResetButtons();
        }
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

    public void ButtonShareFB()
    {
        SetButtonDown(InterfaceButtons.SHARE_FB);
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
        string imageLocation = "ecran_3/";
        switch (v)
        {
            case 3:
                imageLocation += "ecran_3_A";
                break;
            case 2:
                imageLocation += "ecran_3_B";
                break;
            case 1:
                imageLocation += "ecran_3_C";
                break;
        }
        Texture2D s = Resources.Load(imageLocation) as Texture2D;
        if(v != 3)
            Destroy(mCountDown.sprite);
        mCountDown.sprite = Sprite.Create(s, mCountDown.sprite.rect, new Vector2(0.5f, 0.5f));
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
            try
            {
                mOSCController.StartLivePreview();
                mCurrentState = ScreensStates.READY_TAKE_PHOTO;
                UpdateScreen();
            }
            catch(Exception e)
            {
                Debug.Log(e.Message);
            }
        }
    }

    /**
     * If user pushes the Take a Photo button go to screen taking photo
     **/
    private void ManageReadyTakePhotoScreen()
    {
        // For logs
        now = System.DateTime.Now;
        now_str = now.ToString("MM-dd-yyyy_hh.ss.mm");
        log.new_date();
        log.WriteFile(log.file_date_str, "[\n\t{\"event\": \"start\", \"time\": \""+now_str+"\"}," );


        byte[] data = mOSCController.GetLatestData();
        if(data != null)
            mSkyboxMng.DefineNewSkybox(data);

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
        // For logs
        now = System.DateTime.Now;
        now_str = now.ToString("MM-dd-yyyy_hh.ss.mm");
        log.WriteFile(log.file_date_str, "\n\t{\"event\": \"capture\", \"time\": \""+now_str+"\"}," );

        byte[] data = mOSCController.GetLatestData();
        if (data != null)
            mSkyboxMng.DefineNewSkybox(data);

        if (!mCounter.IsCounting())
        {
            StartCoroutine(mCounter.Count(3, UpdateCountDownText));
        }
        else if (mCounter.IsCounterFinished())
        {
            try
            {
                mIsOSCReady = false;
                mOSCController.StopLivePreview();
                mOSCController.StartCapture(TriggerOSCReady);
                mCurrentState = ScreensStates.WAITING;
                UpdateScreen();
            }
            catch(Exception e)
            {
                Debug.Log(e.Message);
            }
            finally
            {
                mCounter = new CounterDown();   //new countdown for next time
            }
        }
    }

    /**
     * Wait until the OSC controller signal that the photo is ready
     * Then retrieve the data, save them and go to display screen with automatic rotation
     **/
    private void ManageWaitingScreen()
    {
        if (mIsOSCReady)
        {
            mFullResolutionImage = mOSCController.GetLatestData();
            mSkyboxMng.DefineNewSkybox(mFullResolutionImage);
            mCamera.AutomaticRotation();
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
        // For logs
        now = System.DateTime.Now;
        now_str = now.ToString("MM-dd-yyyy_hh.ss.mm");
        log.WriteFile(log.file_date_str, "\n\t{\"event\": \"visualize\", \"time\": \""+now_str+"\", \"choice\": " );

        if (IsButtonDown(InterfaceButtons.ABORT))
        {
            // For logs
            log.WriteFile(log.file_date_str, "\"abandon\"}\n]" );

            mSkyboxMng.ResetSkybox();
            mCurrentState = ScreensStates.WELCOME;
            mCamera.AutomaticRotation();
        }
        else if (IsButtonDown(InterfaceButtons.RETRY))
        {
            // For logs
            log.WriteFile(log.file_date_str, "\"restart\"}," );

            mOSCController.StartLivePreview();
            mCurrentState = ScreensStates.READY_TAKE_PHOTO;
        }
        else if (IsButtonDown(InterfaceButtons.OK))
        {
            // For logs
            log.WriteFile(log.file_date_str, "\"share\"}," );

            mCurrentState = ScreensStates.SHARE_PHOTO;
        }
        else
            return;
        UpdateScreen();
    }

    /**
     * On the share screen go back to display screen if user presses the BACK button
     * and disconnect the user from Facebook
     **/
    private void ManageShareScreen()
    {
        // For logs
        now = System.DateTime.Now;
        now_str = now.ToString("MM-dd-yyyy_hh.ss.mm");
        log.WriteFile(log.file_date_str, "\n\t{\"event\": \"share\", \"time\": \""+now_str+"\", \"choice\": " );

        if (IsButtonDown(InterfaceButtons.SHARE_FB))
        {
            // For logs
            log.WriteFile(log.file_date_str, "\"facebook\"}," );

            mWifi.SaveAndShutdownWifi();
            mFB.StartConnection(mFullResolutionImage);
        }

        if (IsButtonDown(InterfaceButtons.BACK))
        {
            // For logs
            log.WriteFile(log.file_date_str, "\"abandon\"}\n]" );

            mWifi.RestoreWifi();
            Thread.Sleep(3000);
            mOSCController.RebootController();
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
     * Start counting from start to 0(excluded) second. Calls update(i) each time
     * This is a coroutine
     **/
    public IEnumerator Count(int start, Action<int> update)
    {
        mCountingDown = true;
        for (int i = start; i > 0; i--)
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
