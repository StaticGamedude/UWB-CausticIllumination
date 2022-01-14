using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraView : MonoBehaviour
{
    public Shader replacementShader;

    private Camera mainCam;

    // Start is called before the first frame update
    void Start()
    {
        mainCam = this.GetComponent<Camera>();

        Debug.Assert(mainCam != null);
        Debug.Assert(replacementShader != null);

        //mainCam.SetReplacementShader(replacementShader, "DrewShader");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
