using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightCam : MonoBehaviour
{
    public Shader replacementShader;

    private Camera lightCam;

    // Start is called before the first frame update
    void Start()
    {
        this.lightCam = this.GetComponent<Camera>();
        this.lightCam.SetReplacementShader(this.replacementShader, "Specular");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
