using System;
using UnityEngine;

/**
 * This class control camera rotations for both automatic and manual mode
 // **/
public class CameraRotation : MonoBehaviour
{
    float mTurnSpeed = 5.0f;
    Vector2 mDelta;
    bool mIsAutomaticRotationEnable = false;
    const float threshold = 2.0f; // value in degrees for camera rotation in automatic mode

    public Transform container; // camera container

    Timeout mScreenTimeout;

    //For logs
    LogSD mLog;
    DateTime mDate = DateTime.Now;

    /* Called once per frame */
    private void Update()
    {
        if (Input.touchCount > 0)
        {
            //For logs
            if (DateTime.Now > mDate.AddSeconds(2))
            {
                //mDate = DateTime.Now;
                //string nowStr = mDate.ToString("dd-MM-yyyy_HH.mm.ss");
                if (mLog.state == LogSD.enum_state.RT)
                {
                    mLog.write_navigate_RT();
                    //mLog.WriteFile(mLog.mFileDataStr, "\t{\"event\": \"navigate_RT\", \"time\": \"" + nowStr + "\"},");
                }
                else if (mLog.state == LogSD.enum_state.HQ)
                {
                    mLog.write_navigate_HD();
                    //mLog.WriteFile(mLog.mFileDataStr, "\t{\"event\": \"navigate_HD\", \"time\": \"" + nowStr + "\"},");
                }

            }

            mScreenTimeout.Reset();
            Vector2 tmp = Input.GetTouch(0).deltaPosition;
            tmp /= 5.0f;
            mDelta.x = tmp.y;
            mDelta.y = -tmp.x;
            ManualRotation();
        }
        else if (Input.GetMouseButton(0))
        {
            mScreenTimeout.Reset();

            mDelta.x = Input.GetAxis("Mouse Y") * 10.0f;
            mDelta.y = -Input.GetAxis("Mouse X") * 10.0f;
            ManualRotation();
        }
        else if (mIsAutomaticRotationEnable)
            mDelta = ComputeDelta();

        Vector2 cam = Vector2.zero, cont = Vector2.zero;
        cam.y = mDelta.y;
        cont.x = mDelta.x;

        //This is made in order to avoid rotation on Z, just typing 0 on Zcoord isn’t enough
        //so the container is rotated around Y and the camera around X separately
        container.Rotate(cam * Time.deltaTime * mTurnSpeed);
        transform.Rotate(cont * Time.deltaTime * mTurnSpeed);

        mDelta = Vector2.zero;
    }

    /* Compute delta vector for automatic rotation */
    private Vector2 ComputeDelta()
    {
        Vector2 d = Vector2.zero;
        d.y = -threshold;
        float actualRotation = transform.rotation.eulerAngles.x;

        if (actualRotation > threshold && (360.0f - actualRotation) > threshold)
            if (actualRotation > 180.0f)
                d.x = threshold;
            else
                d.x = -threshold;
        return d;
    }

    /* Enable automatic rotation */
    public void AutomaticRotation(LogSD logger, Timeout screenTimeout)
    {
        mIsAutomaticRotationEnable = true;
        mLog = logger;
        mScreenTimeout = screenTimeout;
    }

    /* Enable manual rotation */
    public void ManualRotation()
    {
        mIsAutomaticRotationEnable = false;
    }
}
