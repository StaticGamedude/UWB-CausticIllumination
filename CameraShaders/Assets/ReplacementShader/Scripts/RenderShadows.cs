using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderShadows : MonoBehaviour
{
    public Shader ShadowShader;

    public void Start()
    {
        if (ShadowShader != null)
        {
            GetComponent<Camera>().SetReplacementShader(ShadowShader, "RenderType");
        }
    }
}
