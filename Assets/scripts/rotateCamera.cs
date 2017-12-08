using System;
using UnityEngine;

/**
 * This class control camera rotations for both automatic and manual mode
 // **/
public class rotateCamera : MonoBehaviour
{
    float mTurnSpeed = 10.0f;
    Vector2 mDelta;
    bool mIsAutomaticRotationEnable = false;
    const float threshold = 2.0f; // value in degrees for camera rotation in automatic mode

    public Transform container; // camera container

    //For logs
    LogSD log = new LogSD();
    DateTime now;
    String now_str;

    /* Called once per frame */
    private void Update()
    {
        if (Input.touchCount > 0)
        {
          /*
            //For logs
            now = System.DateTime.Now;
            now_str = now.ToString("MM-dd-yyyy_hh.mm.ss");
            log.WriteFile(log.file_date_str, "\t{\"event\": \"navigate_???\", \"time\": \""+now_str+"\"}," );
            */
            mDelta = Input.GetTouch(0).deltaPosition;
            mDelta /= 5.0f;
            ManualRotation();
        }
        else if (Input.GetMouseButton(0))
        {
          /*
            //For logs
            now = System.DateTime.Now;
            now_str = now.ToString("MM-dd-yyyy_hh.mm.ss");

            log.WriteFile(log.file_date_str, "\t{\"event\": \"navigate_?\", \"time\": \""+now_str+"\"}," );
            */
            mDelta = new Vector2(Input.GetAxis("Mouse X") * 10.0f, Input.GetAxis("Mouse Y") * 10.0f);
            ManualRotation();
        }
        else if (mIsAutomaticRotationEnable)
            mDelta = ComputeDelta();

        //This is made in order to avoid rotation on Z, just typing 0 on Zcoord isn’t enough
        //so the container is rotated around Y and the camera around X separately
        container.Rotate(new Vector3(0.0f, -mDelta.x, 0.0f) * Time.deltaTime * mTurnSpeed);
        transform.Rotate(new Vector3(mDelta.y, 0.0f, 0.0f) * Time.deltaTime * mTurnSpeed);

        mDelta = Vector2.zero;
    }

    /* Compute delta vector for automatic rotation */
    private Vector2 ComputeDelta()
    {
        Vector2 d = Vector2.zero;
        d.x = threshold;
        float actualRotation = transform.rotation.eulerAngles.x;

        if (actualRotation > threshold && (360.0f - actualRotation) > threshold)
            if (actualRotation > 180.0f)
                d.y = threshold;
            else
                d.y = -threshold;
        return d;
    }

    /* Enable automatic rotation */
    public void AutomaticRotation(String date_str)
    {
        mIsAutomaticRotationEnable = true;
        log.file_date_str = date_str;
    }

    /* Enable manual rotation */
    public void ManualRotation()
    {
        mIsAutomaticRotationEnable = false;
    }
}
