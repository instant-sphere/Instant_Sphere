using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rotateCamera : MonoBehaviour {
    float mTurnSpeedMouse = 10.0f;
    Vector2 mDelta;
    bool mAutomaticRotation = true;

    public Transform container;

    void Update()
    {
        if (mAutomaticRotation)
            SetDelta();
        else
            mDelta = Input.GetTouch(0).deltaPosition;

        //This is made in order to avoid rotation on Z, just typing 0 on Zcoord isn’t enough
        //so the container is rotated around Y and the camera around X separately
        container.Rotate(new Vector3(0, mDelta.x * (-1), 0f) * Time.deltaTime * mTurnSpeedMouse);
        transform.Rotate(new Vector3(mDelta.y, 0, 0) * Time.deltaTime * mTurnSpeedMouse);
    }

    private void SetDelta()
    {
        mDelta.x = 3.0f;
        mDelta.y = 0.0f;
    }

    public void AutomaticRotation()
    {
        mAutomaticRotation = true;
    }

    public void ManualRotation()
    {
        mAutomaticRotation = false;
    }
}
