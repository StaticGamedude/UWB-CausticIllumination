using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultCameraTest : MonoBehaviour
{
    public Shader TestShader;

    private Camera cam;
    // Start is called before the first frame update
    void Start()
    {
        cam = this.GetComponent<Camera>();

        Debug.Assert(cam != null);
        Debug.Assert(TestShader != null);

        cam.SetReplacementShader(TestShader, "SpecularObj");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
