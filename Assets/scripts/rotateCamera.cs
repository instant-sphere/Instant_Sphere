﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rotateCamera : MonoBehaviour {
    float horizontal;
    float vertical;
    public Transform container;
    float turnSpeedMouse = 100.0f;

    void Update()
    {
        //Using mouse
        horizontal = Input.GetAxis("Mouse X");
        vertical = Input.GetAxis("Mouse Y");

        //This is made in order to avoid rotation on Z, just by typing 0 on Zcoord isn’t enough
        //so the container is rotated around Y and the camera around X separately
        container.Rotate(new Vector3(0, horizontal * (-1), 0f) * Time.deltaTime * turnSpeedMouse);
        transform.Rotate(new Vector3(vertical, 0, 0) * Time.deltaTime * turnSpeedMouse);

    }
}
