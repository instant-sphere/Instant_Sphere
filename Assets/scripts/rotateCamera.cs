using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rotateCamera : MonoBehaviour {
    float horizontal;
    float vertical;
    float turnSpeedMouse = 10.0f;
    public Transform container;

    void Update()
    {
        Vector2 finger = Input.GetTouch(0).deltaPosition;

        //This is made in order to avoid rotation on Z, just typing 0 on Zcoord isn’t enough
        //so the container is rotated around Y and the camera around X separately
        container.Rotate(new Vector3(0, finger.x * (-1), 0f) * Time.deltaTime * turnSpeedMouse);
        transform.Rotate(new Vector3(finger.y, 0, 0) * Time.deltaTime * turnSpeedMouse);
    }
}
