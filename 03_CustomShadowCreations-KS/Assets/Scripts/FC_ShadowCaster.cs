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
public class FC_ShadowCaster : MonoBehaviour {
    private const float SHADOW_BIAS = 0.005f;
    public Texture DepthTexture;
    public Shader FC_ObjShader;

    public bool ShowCamInWC = true;
    public bool ShowCamInProj = false;

    private GameObject mEyePt;
    private GameObject mAtPt;
    private GameObject[] mFrustumPts;

    private GameObject mEyeInProj;
    private GameObject mAtInProj;
    private GameObject[] mFrustumInProj;

    private string[] mNames = { "TR", "TL", "BR", "BL" };

    private Matrix4x4 shadowMatrix = Matrix4x4.identity;
    Matrix4x4 bias = new Matrix4x4()
    {
        m00 = 0.5f,        m01 = 0,        m02 = 0,        m03 = 0.5f,
        m10 = 0,           m11 = 0.5f,     m12 = 0,        m13 = 0.5f,
        m20 = 0,           m21 = 0,        m22 = 0.5f,     m23 = 0.5f,
        m30 = 0,           m31 = 0,        m32 = 0,        m33 = 1,
    };

    private Camera _cam;

    // Start is called before the first frame update
    void Start()
    {
        _cam = GetComponent<Camera>();
        Debug.Assert(DepthTexture != null);
        Debug.Assert(FC_ObjShader != null);
        Debug.Assert(_cam != null);
        _cam.SetReplacementShader(FC_ObjShader, "FC_ShadowProducer");

        float scale = 2f;
        mEyePt = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        mEyePt.transform.localScale = new Vector3(scale, scale, scale);
        mEyePt.GetComponent<Renderer>().material.color = Color.red;
        mEyePt.gameObject.name = "Eye(WC)";

        mAtPt = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        mAtPt.transform.localScale = new Vector3(scale, scale, scale);
        mAtPt.GetComponent<Renderer>().material.color = Color.black ;
        mAtPt.gameObject.name = "At(WC)";

        scale = 1f;
        mFrustumPts = new GameObject[8]; // 4 for near and 4 for far
        scale = 1f;
        for (int i = 0; i < 8; i++)
        {
            mFrustumPts[i] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            mFrustumPts[i].transform.localScale = new Vector3(scale, scale, scale);
            mFrustumPts[i].GetComponent<Renderer>().material.color = Color.blue;

            string s = (i >= 4) ? "Far:" : "Near:";
            mFrustumPts[i].gameObject.name = s + mNames[i % 4];
        }


        // in projection space
        scale = 0.1f;
        mEyeInProj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        mEyeInProj.transform.localScale = new Vector3(scale, scale, scale);
        mEyeInProj.GetComponent<Renderer>().material.color = Color.red;
        mEyeInProj.gameObject.name = "Eye(Proj)";

        scale = 0.1f;
        mAtInProj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        mAtInProj.transform.localScale = new Vector3(scale, scale, scale);
        mAtInProj.GetComponent<Renderer>().material.color = Color.black;
        mAtInProj.gameObject.name = "At(Proj)";

        scale = 0.1f;
        mFrustumInProj = new GameObject[8]; // 4 for near and 4 for far
        for (int i = 0; i<8; i++)
        {
            mFrustumInProj[i] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            mFrustumInProj[i].transform.localScale = new Vector3(scale, scale, scale);
            if (i < 4)
                mFrustumInProj[i].GetComponent<Renderer>().material.color = Color.blue;
            else
                mFrustumInProj[i].GetComponent<Renderer>().material.color = Color.white;

            string s = (i >= 4) ? "Far:" : "Near:";
            mFrustumInProj[i].gameObject.name = s + mNames[i % 4];
        }

    }

    private Vector3 mul(Vector3 p)
    {
        Vector4 pt = p;
        pt.w = 1;
        pt = shadowMatrix * pt;
        // pt.w = 1;
        return new Vector3(pt.x / pt.w, pt.y/pt.w, pt.z / pt.w);
    }

    private void Update()
    {
        mEyePt.SetActive(ShowCamInWC);
        mAtPt.SetActive(ShowCamInWC);

        mEyeInProj.SetActive(ShowCamInProj);
        mAtInProj.SetActive(ShowCamInProj);

        Vector3 localF = _cam.transform.rotation * Vector3.forward;
        Vector3 localY = _cam.transform.rotation * Vector3.up;
        Vector3 localX = _cam.transform.rotation * Vector3.right;

        // positions along the viewing direction
        Vector3 centerAtNear = _cam.transform.position + _cam.nearClipPlane * localF;
        Vector3 centerAtFar = _cam.transform.position + _cam.farClipPlane * localF;

        mEyePt.transform.localPosition = _cam.transform.position; 
        mAtPt.transform.localPosition = centerAtFar;

        mEyeInProj.transform.localPosition = mul(centerAtNear);
        mAtInProj.transform.localPosition = mul(mAtPt.transform.localPosition);
        
        // dimension of the near and far plane
        float vFov_2 = 0.5f * _cam.fieldOfView * Mathf.Deg2Rad;
        float hFov_2 = vFov_2 * _cam.aspect;
        float nearH_2 = _cam.nearClipPlane * Mathf.Tan(vFov_2);
        float nearW_2 = _cam.nearClipPlane * Mathf.Tan(hFov_2);
        float farH_2 = _cam.farClipPlane * Mathf.Tan(vFov_2);
        float farW_2 = _cam.farClipPlane * Mathf.Tan(hFov_2);
        
        // Near plane positions
        mFrustumPts[0].transform.localPosition = centerAtNear + nearH_2 * localY + nearW_2 * localX;
        mFrustumPts[1].transform.localPosition = centerAtNear + nearH_2 * localY - nearW_2 * localX;
        mFrustumPts[2].transform.localPosition = centerAtNear - nearH_2 * localY + nearW_2 * localX;
        mFrustumPts[3].transform.localPosition = centerAtNear - nearH_2 * localY - nearW_2 * localX;

        // Far plane positions
        mFrustumPts[4].transform.localPosition = centerAtFar + farH_2 * localY + farW_2 * localX;
        mFrustumPts[5].transform.localPosition = centerAtFar + farH_2 * localY - farW_2 * localX;
        mFrustumPts[6].transform.localPosition = centerAtFar - farH_2 * localY + farW_2 * localX;
        mFrustumPts[7].transform.localPosition = centerAtFar - farH_2 * localY - farW_2 * localX;

        for (int i= 0; i<8; i++)
        {
            mFrustumPts[i].SetActive(ShowCamInWC);
            mFrustumInProj[i].SetActive(ShowCamInProj);

            mFrustumInProj[i].transform.localPosition = mul(mFrustumPts[i].transform.localPosition);
        }

        if (ShowCamInProj)
        {
            // { "TR", "TL", "BR", "BL" };
            // 0 = Near
            // 4 = Far
            for (int i = 0; i < 7; i += 4)
            {
                Debug.DrawLine(mFrustumInProj[i + 0].transform.localPosition, mFrustumInProj[i + 1].transform.localPosition);
                Debug.DrawLine(mFrustumInProj[i + 0].transform.localPosition, mFrustumInProj[i + 2].transform.localPosition);
                Debug.DrawLine(mFrustumInProj[i + 1].transform.localPosition, mFrustumInProj[i + 3].transform.localPosition);
                Debug.DrawLine(mFrustumInProj[i + 2].transform.localPosition, mFrustumInProj[i + 3].transform.localPosition);
            }

            // Connect Near Far
            for (int i = 0; i <= 3; i++)
                Debug.DrawLine(mFrustumInProj[i].transform.localPosition, mFrustumInProj[i + 4].transform.localPosition);
        }
    }

    private void OnPostRender()
    {
        shadowMatrix = bias * _cam.projectionMatrix * _cam.worldToCameraMatrix;
        Shader.SetGlobalMatrix("_FC_ShadowCamMatrix", _cam.worldToCameraMatrix);
        Shader.SetGlobalFloat("_FC_ShadowCam_Far", 1.0f/_cam.farClipPlane);
        Shader.SetGlobalMatrix("_FC_ShadowMatrix", shadowMatrix);
        Shader.SetGlobalTexture("_FC_ShadowTex", DepthTexture);
        Shader.SetGlobalFloat("_FC_ShadowBias", SHADOW_BIAS);
    }
}