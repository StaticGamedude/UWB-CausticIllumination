/* UWB Caustic Illumination Research, 2021
 * Participants: Drew Nelson, Dr. Kelvin Sung
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Shadow generation options to support in the world
/// </summary>
public enum ShadowOptions
{
    UNITY = 0, //Built-in unity shadows. Uses default materials with shadows enabled
    UNITY_DEPTH, //Follows the third-party implementation of shadows. Shaders used on receving objects and render texture created on camera script
    FULL_CUSTOM //Full custom implementation. Utilizes replacement shaders and custom shaders
}

/// <summary>
/// Behavior of the world. Controls which material each object uses to help create the desired shadow effect
/// </summary>
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

    /// <summary>
    /// Request the world to change the shadow rendering type
    /// </summary>
    /// <param name="option">Desired shadow rendering option</param>
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

    /// <summary>
    /// Setup the shadow and receiving objects to use Unity's default materials and lighting
    /// </summary>
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

    /// <summary>
    /// Setup the receiving objects to use the third party (TP) shaders. Shadow objects use Unity's default material with 
    /// shadows turned off
    /// </summary>
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

    /// <summary>
    /// Setup the shadow and receiving object to use fully custom (FP) shaders.
    /// </summary>
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

    /// <summary>
    /// Get the current shadow rendering type in the world.
    /// </summary>
    /// <returns></returns>
    public ShadowOptions GetCurrentShadowOption()
    {
        return _currentShadowOption;
    }
}
