using System;
using System.Collections;
using UnityEngine;

public class Timeout
{
    float mTimeBeforeTrigger;
    float mTotalTime;
    Action mCallback;

    public Timeout(float count, Action cb)
    {
        mTimeBeforeTrigger = count;
        mTotalTime = count;
        mCallback = cb;
    }

    public void Reset()
    {
        mTimeBeforeTrigger = mTotalTime;
    }

    public IEnumerable StartTimer()
    {
        while (mTimeBeforeTrigger > 0.0f)
        {
            mTimeBeforeTrigger -= 1.0f;
            yield return new WaitForSecondsRealtime(1.0f);
        }
        mCallback();
    }
}
