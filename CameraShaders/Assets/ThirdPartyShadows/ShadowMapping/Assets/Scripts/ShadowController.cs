using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowController : MonoBehaviour
{
    public Transform shadowReceiverPlane;

    private MeshRenderer meshRender;
    // Start is called before the first frame update
    void Awake()
    {
        meshRender = GetComponent<MeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (shadowReceiverPlane != null)
        {
            Vector4 planeNormal = new Vector4(
                shadowReceiverPlane.transform.up.x,
                shadowReceiverPlane.transform.up.y,
                shadowReceiverPlane.transform.up.z,
                -Vector3.Dot(shadowReceiverPlane.transform.up, shadowReceiverPlane.transform.position)
            );

            meshRender.sharedMaterial.SetVector("_PlaneNormal", planeNormal);
        }
    }
}
