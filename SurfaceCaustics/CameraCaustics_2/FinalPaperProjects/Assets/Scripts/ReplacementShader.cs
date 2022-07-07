using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReplacementShader : MonoBehaviour
{
    public Shader replaceShader;

    // Start is called before the first frame update
    void Start()
    {
        Camera currentCam = this.GetComponent<Camera>();
        currentCam.SetReplacementShader(this.replaceShader, "Test");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
