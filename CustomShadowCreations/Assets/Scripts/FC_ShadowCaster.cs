/* UWB Caustic Illumination Research, 2021
 * Participants: Drew Nelson, Dr. Kelvin Sung
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Full custom shadow renderer. Utilizes a replacement shader on the depth camera which is rendered to a texture.
/// Texture is loaded globally into shaders on post render. Shadow matrix and shadow bias also loaded in shaders on
/// post render
/// </summary>
public class FC_ShadowCaster : MonoBehaviour
{
    private const float SHADOW_BIAS = 0.005f;
    public Texture DepthTexture;
    public Shader FC_ObjShader;

    private Camera _cam;

    // Start is called before the first frame update
    void Start()
    {
        _cam = GetComponent<Camera>();
        Debug.Assert(DepthTexture != null);
        Debug.Assert(FC_ObjShader != null);
        Debug.Assert(_cam != null);
        _cam.SetReplacementShader(FC_ObjShader, "FC_ShadowProducer");
    }

    private void OnPostRender()
    {
        Matrix4x4 bias = new Matrix4x4()
        {
            m00 = 0.5f, m01 = 0,    m02 = 0,    m03 = 0.5f,
            m10 = 0,    m11 = 0.5f, m12 = 0,    m13 = 0.5f, 
            m20 = 0,    m21 = 0,    m22 = 1f,   m23 = 0f,
            m30 = 0,    m31 = 0,    m32 = 0,    m33 = 1,
        };

        Matrix4x4 shadowMatrix = bias * _cam.projectionMatrix * _cam.worldToCameraMatrix;
        Shader.SetGlobalMatrix("_FC_ShadowMatrix", shadowMatrix);
        Shader.SetGlobalTexture("_FC_ShadowTex", DepthTexture);
        Shader.SetGlobalFloat("_FC_ShadowBias", SHADOW_BIAS);
    }
}