using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanarReflectionManager : MonoBehaviour
{
    public GameObject _reflectionPlane;

    private Camera _mirrorCam;
    private Camera _mainCam;

    RenderTexture _renderTarget;

    public Material _floorMaterial;

    [Range(0.0f, 1.0f)]
    public float _reflectionFactor = 0.5f;

    // Start is called before the first frame update
    void Start()
    {
        GameObject cameraGameObject = new GameObject("ReflectionCamera");
        _mirrorCam = cameraGameObject.AddComponent<Camera>();
        _mirrorCam.enabled = false;

        _mainCam = Camera.main;

        _renderTarget = new RenderTexture(Screen.width, Screen.height, 24);
    }

    // Update is called once per frame
    void Update()
    {
        Shader.SetGlobalFloat("_ReflectionFactor", _reflectionFactor);
    }

    private void OnPostRender()
    {
        RenderReflection();
    }

    void RenderReflection()
    {
        _mirrorCam.CopyFrom(_mainCam);

        Vector3 cameraDirectionWorldSpace = _mainCam.transform.forward;
        Vector3 cameraUpWorldSpace = _mainCam.transform.up;
        Vector3 cameraPositionWorldSpace = _mainCam.transform.position;

        //Transform the vector's to the floor space
        Vector3 cameraDirectionPlaneSpace = _reflectionPlane.transform.InverseTransformDirection(cameraDirectionWorldSpace);
        Vector3 cameraUpPlaneSpace = _reflectionPlane.transform.InverseTransformDirection(cameraUpWorldSpace);
        Vector3 cameraPositionPlaneSpace = _reflectionPlane.transform.InverseTransformDirection(cameraPositionWorldSpace);

        //Mirror the vectors
        cameraDirectionPlaneSpace.y *= -1.0f;
        cameraUpPlaneSpace.y *= -1.0f;
        cameraPositionPlaneSpace.y *= -1.0f;

        //Transform vectors back to world space
        cameraDirectionWorldSpace = _reflectionPlane.transform.TransformDirection(cameraDirectionPlaneSpace);
        cameraUpWorldSpace = _reflectionPlane.transform.TransformDirection(cameraUpPlaneSpace);
        cameraPositionWorldSpace = _reflectionPlane.transform.TransformDirection(cameraPositionPlaneSpace);

        //Set the camera position position and rotation;
        _mirrorCam.transform.position = cameraPositionWorldSpace;
        _mirrorCam.transform.LookAt(cameraPositionWorldSpace + cameraDirectionWorldSpace, cameraUpWorldSpace);

        //Set the render target of the relection camera
        _mirrorCam.targetTexture = _renderTarget;

        //Render the reflection camera
        _mirrorCam.Render();

        //Draw full screen quad
        DrawQuad();
    }

    void DrawQuad()
    {
        GL.PushMatrix();

        //Use ground material to draw the quad
        _floorMaterial.SetPass(0);
        _floorMaterial.SetTexture("_ReflectionTex", _renderTarget);


        GL.LoadOrtho();

        GL.Begin(GL.QUADS);

        GL.TexCoord2(1.0f, 0.0f);
        GL.Vertex3(0.0f, 0.0f, 0.0f);
        GL.TexCoord2(1.0f, 1.0f);
        GL.Vertex3(0.0f, 1.0f, 0.0f);
        GL.TexCoord2(0.0f, 1.0f);
        GL.Vertex3(1.0f, 1.0f, 0.0f);
        GL.TexCoord2(0.0f, 0.0f);
        GL.Vertex3(1.0f, 0.0f, 0.0f);

        GL.End();

        GL.PopMatrix();
    }
}
