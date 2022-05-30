using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    public float RefractionIndex = 1;

    public int VisibileSurfaceArea = 600;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Shader.SetGlobalFloat("_RefractionIndex", this.RefractionIndex);
        Shader.SetGlobalInt("_VisibleSurfaceArea", this.VisibileSurfaceArea);
    }
}
