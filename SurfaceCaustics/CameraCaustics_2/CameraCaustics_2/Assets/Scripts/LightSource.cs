using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightSource : MonoBehaviour
{
    private Camera lightCamera;

    // Start is called before the first frame update
    void Start()
    {
        this.lightCamera = this.InstantiateCausticCamera(this.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        Shader.SetGlobalVector("_LightPosition", this.transform.position);
        Shader.SetGlobalMatrix($"_LightViewProjectionMatrix", this.lightCamera.projectionMatrix * lightCamera.worldToCameraMatrix);
    }

    private Camera InstantiateCausticCamera(GameObject parentObj)
    {
        GameObject cameraObject = new GameObject("LightCam");
        Camera camera = cameraObject.AddComponent<Camera>();

        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0, 0, 0, 0);
        cameraObject.transform.parent = parentObj.transform;
        cameraObject.transform.localPosition = new Vector3();
        cameraObject.transform.localRotation = Quaternion.identity;


        return camera;
    }
}
