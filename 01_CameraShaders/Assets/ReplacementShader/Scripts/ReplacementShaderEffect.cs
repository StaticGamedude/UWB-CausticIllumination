using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReplacementShaderEffect : MonoBehaviour
{
    public Shader ReplacementShader;
    public Material DepthMat;

    private void OnEnable()
    {
        if (ReplacementShader != null)
        {
            GetComponent<Camera>().SetReplacementShader(ReplacementShader, "DrewShader");
        }
    }

    private void OnDisable()
    {
        GetComponent<Camera>().ResetReplacementShader();
    }

    private void Update()
    {
        Texture texture = GetComponent<Camera>().targetTexture;
        Shader.SetGlobalTexture("_DepthTex", texture);
    }
}
