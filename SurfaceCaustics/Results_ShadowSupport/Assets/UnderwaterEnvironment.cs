using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnderwaterEnvironment : MonoBehaviour
{
    private Camera currentCamera;
    public List<Camera> ExtraCameras = new List<Camera>();

    // Start is called before the first frame update
    void Start()
    {
        this.currentCamera = Camera.main;
        this.ExtraCameras.Insert(0, Camera.main);
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
        
        if (desiredCameraIndex >= 0 && desiredCameraIndex < this.ExtraCameras.Count)
        {
            Camera cam = this.ExtraCameras[desiredCameraIndex];

            cam.enabled = true;

            this.currentCamera.enabled = false;
            this.currentCamera = cam;
        }
    }
}
