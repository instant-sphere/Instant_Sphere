using UnityEngine;

/* This class control camera rotations for both automatic and manual mode */
public class rotateCamera : MonoBehaviour {
    float mTurnSpeedMouse = 10.0f;
    Vector2 mDelta;
    bool mIsAutomaticRotationEnable = false;
    const float treshold = 2.0f;

    public Transform container;

    /* Called once per frame */
    void Update()
    {
        if (mIsAutomaticRotationEnable)
            mDelta = ComputeDelta();
        else if (Input.touchCount > 0)
            mDelta = Input.GetTouch(0).deltaPosition;

        //This is made in order to avoid rotation on Z, just typing 0 on Zcoord isn’t enough
        //so the container is rotated around Y and the camera around X separately
        //container.Rotate(new Vector3(0.0f, - mDelta.x, 0.0f) * Time.deltaTime * mTurnSpeedMouse);
        transform.Rotate(new Vector3(mDelta.y, 0.0f, 0.0f) * Time.deltaTime * mTurnSpeedMouse);

        mDelta = Vector2.zero;
    }

    /* Compute delta vector for automatic rotation */
    Vector2 ComputeDelta()
    {
        Vector2 d = Vector2.zero;
        d.x = 2.5f;
        float actualRotation = transform.rotation.eulerAngles.x;
        
        if (actualRotation != 0.0f)
            if (actualRotation > treshold || (360.0f - actualRotation) > treshold)
                if (actualRotation > 180.0f)
                    d.y = treshold;
                else
                    d.y = -treshold;
            else
               d.y = -actualRotation;
        return d;
    }

    /* Enable automatic rotation */
    public void AutomaticRotation()
    {
        mIsAutomaticRotationEnable = true;
    }

    /* Enable manual rotation */
    public void ManualRotation()
    {
        mIsAutomaticRotationEnable = false;
    }
}
