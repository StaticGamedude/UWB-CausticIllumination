using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    public float RefractionIndex = 1;

    public int VisibileSurfaceArea = 600;

    public float AbsorbtionCoefficient = 0.00017f;

    public float FluxMultiplier = 500;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Shader.SetGlobalFloat("_RefractionIndex", this.RefractionIndex);
        Shader.SetGlobalInt("_VisibleSurfaceArea", this.VisibileSurfaceArea);
        Shader.SetGlobalFloat("_AbsorbtionCoefficient", this.AbsorbtionCoefficient);
        Shader.SetGlobalFloat("_FluxMultiplier", this.FluxMultiplier);
    }
}
