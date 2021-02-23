using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowCaster : MonoBehaviour
{
    private const int DEPTH_TEXTURE_SIZE = 512;
    private const float SHADOW_BIAS = 0.005f;
    private Camera _cam;
    private RenderTexture _depthTexture;

    // Start is called before the first frame update
    void Start()
    {
        _cam = GetComponent<Camera>();
        Debug.Assert(_cam != null);

        _depthTexture = new RenderTexture(DEPTH_TEXTURE_SIZE, DEPTH_TEXTURE_SIZE, 16, RenderTextureFormat.Depth, RenderTextureReadWrite.Linear);
        _depthTexture.wrapMode = TextureWrapMode.Clamp;
        _depthTexture.filterMode = FilterMode.Bilinear;
        _depthTexture.autoGenerateMips = false;
        _depthTexture.useMipMap = false;
        _cam.targetTexture = _depthTexture;
    }

    private void OnPostRender()
    {
        Matrix4x4 bias = new Matrix4x4()
        {
            m00 = 0.5f, m01 = 0,    m02 = 0,    m03 = 0.5f,
            m10 = 0,    m11 = 0.5f, m12 = 0,    m13 = 0.5f,
            m20 = 0,    m21 = 0,    m22 = 0.5f, m23 = 0.5f,
            m30 = 0,    m31 = 0,    m32 = 0,    m33 = 1,
        };

        Matrix4x4 shadowMatrix = bias * _cam.projectionMatrix * _cam.worldToCameraMatrix;
        Shader.SetGlobalMatrix("_ShadowMatrix", shadowMatrix);
        Shader.SetGlobalTexture("_ShadowTex", _depthTexture);
        Shader.SetGlobalFloat("_ShadowBias", SHADOW_BIAS);   
    }
}
