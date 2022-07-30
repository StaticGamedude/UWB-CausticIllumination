using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnderwaterEnvironment : MonoBehaviour
{
    private Camera currentCamera;
    public List<Camera> AllCameras = new List<Camera>();

    // Start is called before the first frame update
    void Start()
    {
        this.currentCamera = Camera.main;

        for(int i = 0; i < this.AllCameras.Count; i++)
        {
            if (i == 0)
            {
                this.AllCameras[i].enabled = true;
            }
            else
            {
                this.AllCameras[i].enabled = false;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        int desiredCameraIndex = -1;
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            desiredCameraIndex = 0;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            desiredCameraIndex = 1;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            desiredCameraIndex = 2;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            desiredCameraIndex = 3;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            desiredCameraIndex = 4;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            desiredCameraIndex = 5;
        }
        

        if (desiredCameraIndex >= 0 && desiredCameraIndex < this.AllCameras.Count)
        {
            Camera cam = this.AllCameras[desiredCameraIndex];

            // "Turn off" the current camera first.
            currentCamera.enabled = false;

            // "Turn on" the new desired camera
            cam.enabled = true;

            // Update the current camera reference
            this.currentCamera = cam;
        }
    }
}
