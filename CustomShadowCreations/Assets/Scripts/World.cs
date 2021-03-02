using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ShadowOptions
{
    UNITY = 0,
    UNITY_DEPTH,
    FULL_CUSTOM
}

public class World : MonoBehaviour
{
    public List<GameObject> ShadowCastingObjects;
    public List<GameObject> ShadowReceivingObjects;
    public GameObject UnityDirectionalLight;
    public Material Default_ShadowProducerMat;
    public Material TP_ShadowReceiverMat;
    public Material FC_ShadowProducerMat;
    public Material FC_ShadowReceiverMat;

    private ShadowOptions _currentShadowOption = ShadowOptions.UNITY;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Assert(ShadowCastingObjects != null);
        Debug.Assert(ShadowReceivingObjects != null);
        Debug.Assert(UnityDirectionalLight);
        Debug.Assert(Default_ShadowProducerMat != null);
        Debug.Assert(TP_ShadowReceiverMat != null);
        Debug.Assert(FC_ShadowProducerMat != null);
        Debug.Assert(FC_ShadowReceiverMat != null);

        SetShadowOption(_currentShadowOption);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetShadowOption(ShadowOptions option)
    {
        if (_currentShadowOption != option)
        {
            switch(option)
            {
                case ShadowOptions.UNITY:
                    ConfigureWorldForUnityShadows();
                    break;
                case ShadowOptions.UNITY_DEPTH:
                    ConfigureWorldForUnityDepthMap();
                    break;
                case ShadowOptions.FULL_CUSTOM:
                    ConfigureWorldForFullCustom();
                    break;
            }
        }
    }

    private void ConfigureWorldForUnityShadows()
    {
        foreach(GameObject caster in ShadowCastingObjects)
        {
            MeshRenderer mr = caster.GetComponent<MeshRenderer>();
            mr.material = Default_ShadowProducerMat;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        }

        foreach(GameObject receiver in ShadowReceivingObjects)
        {
            MeshRenderer mr = receiver.GetComponent<MeshRenderer>();
            mr.material = new Material(Shader.Find("Diffuse"));
        }
        _currentShadowOption = ShadowOptions.UNITY;
    }

    private void ConfigureWorldForUnityDepthMap()
    {
        foreach (GameObject o in ShadowCastingObjects)
        {
            MeshRenderer mr = o.GetComponent<MeshRenderer>();
            mr.material = Default_ShadowProducerMat;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        foreach (GameObject receiver in ShadowReceivingObjects)
        {
            MeshRenderer mr = receiver.GetComponent<MeshRenderer>();
            mr.material = TP_ShadowReceiverMat;
        }
        _currentShadowOption = ShadowOptions.UNITY_DEPTH;
    }

    private void ConfigureWorldForFullCustom()
    {
        foreach (GameObject o in ShadowCastingObjects)
        {
            MeshRenderer mr = o.GetComponent<MeshRenderer>();
            mr.material = FC_ShadowProducerMat;
        }

        foreach (GameObject receiver in ShadowReceivingObjects)
        {
            MeshRenderer mr = receiver.GetComponent<MeshRenderer>();
            mr.material = FC_ShadowReceiverMat;
        }
        _currentShadowOption = ShadowOptions.FULL_CUSTOM;
    }

    public ShadowOptions GetCurrentShadowOption()
    {
        return _currentShadowOption;
    }
}
