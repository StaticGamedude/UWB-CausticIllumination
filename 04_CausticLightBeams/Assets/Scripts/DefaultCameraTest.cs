using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultCameraTest : MonoBehaviour
{
    private const float SHADOW_BIAS = 0.005f;
    public Shader TestShader;
    public Texture targetTexture;

    private Camera lightCamera;
    

    // Start is called before the first frame update
    void Start()
    {
        lightCamera = this.GetComponent<Camera>();

        Debug.Assert(lightCamera != null);
        Debug.Assert(TestShader != null);

        lightCamera.SetReplacementShader(TestShader, "SpecularObj");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnPostRender()
    {
        Matrix4x4 bias = new Matrix4x4()
        {
            m00 = 0.5f,
            m01 = 0,
            m02 = 0,
            m03 = 0.5f,
            m10 = 0,
            m11 = 0.5f,
            m12 = 0,
            m13 = 0.5f,
            m20 = 0,
            m21 = 0,
            m22 = 0.5f,
            m23 = 0.5f,
            m30 = 0,
            m31 = 0,
            m32 = 0,
            m33 = 1,
        };

        Matrix4x4 lightMatrix = bias * lightCamera.projectionMatrix * lightCamera.worldToCameraMatrix;

        Shader.SetGlobalTexture("_SpecularPosTexture", targetTexture);
        Shader.SetGlobalMatrix("LightMatrix", lightMatrix);
        Shader.SetGlobalFloat("LightBias", SHADOW_BIAS);
    }
}
