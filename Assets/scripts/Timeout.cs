using System;
using System.Collections;
using UnityEngine;

/**
 * This class represents a timeout which calls a function when reaching 0
 **/
public class Timeout
{
    float mTimeBeforeTrigger;   //time remaining
    float mTotalTime;           //total time
    Action mCallback;           //callback

    /**
     * Constructor, count is the timeout total time, cb is the callback function
     **/
    public Timeout(float count, Action cb)
    {
        mTimeBeforeTrigger = count;
        mTotalTime = count;
        mCallback = cb;
    }

    /**
     * Reset the counter to its total time
     **/
    public void Reset()
    {
        mTimeBeforeTrigger = mTotalTime;
    }

    /**
     * Starts the timer and calls the callback when it reaches 0
     **/
    public IEnumerator StartTimer()
    {
        while (mTimeBeforeTrigger > 0.0f)
        {
            mTimeBeforeTrigger -= 1.0f;
            yield return new WaitForSecondsRealtime(1.0f);
        }
        Logger.Instance.WriteTimeout();
        mCallback();
    }
}
