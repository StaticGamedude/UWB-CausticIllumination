using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetCausticFlags : MonoBehaviour
{
    public bool IsSpecularObject = false;

    public bool IsReceivingObject = false;

    public bool flagsSet = false;

    // Start is called before the first frame update
    void Awake()
    {
        Renderer objectRenderer = this.GetComponent<Renderer>();
        if (this.IsSpecularObject)
        {
            objectRenderer.material.SetOverrideTag("SpecularObj", "1");
        }

        if (this.IsReceivingObject)
        {
            objectRenderer.material.SetOverrideTag("SpecularReceiver", "1");
        }

    }

    private void Update()
    {

    }
}
