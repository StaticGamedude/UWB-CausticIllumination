using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    private RenderTexture m_paintAccumulationRT;

    private Material m_MaterialData;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void ConstructRenderTargets()
    {
        m_paintAccumulationRT = new RenderTexture(256, 256, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);// Must be ARGB32 but will get automagically treated as float or float4 or int or half, from your shader code declaration.
        m_paintAccumulationRT.name = _MainTexInternal;
        m_paintAccumulationRT.enableRandomWrite = true;
        m_paintAccumulationRT.Create();

        m_MaterialData.material.SetTexture(m_MaterialData.sp_MainTexInternal, m_paintAccumulationRT);
        m_MaterialData.material.SetTexture(m_MaterialData.sp_MainTexInternal_Sampler2D, m_paintAccumulationRT);
        Graphics.ClearRandomWriteTargets();
        Graphics.SetRandomWriteTarget(2, m_paintAccumulationRT);//with `, true);` it doesn't take RTs
    }
}
