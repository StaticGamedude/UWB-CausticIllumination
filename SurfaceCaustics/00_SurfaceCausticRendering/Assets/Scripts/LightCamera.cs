using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Contains the logical operations to support the functionality of a camera which acts in place of a light source.
/// </summary>
public class LightCamera : MonoBehaviour
{

    #region Variables Set In The Unity Editor

    /// <summary>
    /// Shader that should be used for the camera when rendering specular objects. Specular objects are expected to have the
    /// tag <see cref="Globals.SPECULAR_OBJECT_SHADER_TAG"/>
    /// </summary>
    public Shader SpecularObjectShader;

    /// <summary>
    /// Texture that receives information based on what the camera can see. Texture is expected to contain either the 
    /// world positions or world normals of the verticies of the objects that the camera can see. Texture is expected 
    /// to be set in the unity editor
    /// </summary>
    public Texture DataTexture;

    /// <summary>
    /// Indicates the <see cref="CameraType" /> of the attached camera. Expected to be set in the Unity editor
    /// </summary>
    public LightCameraType CamType;

    #endregion

    /// <summary>
    /// Camera in which this script is attached to
    /// </summary>
    private Camera lightCamera;


    // Start is called before the first frame update
    void Start()
    {
        this.lightCamera = this.GetComponent<Camera>();

        Debug.Assert(this.lightCamera != null);
        Debug.Assert(this.SpecularObjectShader != null);
        Debug.Assert(this.DataTexture != null);

        this.lightCamera.SetReplacementShader(SpecularObjectShader, Globals.SPECULAR_OBJECT_SHADER_TAG);
    }

    /// <summary>
    /// Create the light matrix which will help us transform vertex positions/normals into the light's point of view
    /// </summary>
    private void OnPostRender()
    {
        Matrix4x4 lightMatrix = Globals.BIAS * this.lightCamera.projectionMatrix * lightCamera.worldToCameraMatrix;
        string shaderTextureParameter = this.CamType == LightCameraType.NORMAL ? Globals.SHADER_PARAM_POSITION_TEXTURE : Globals.SHADER_PARAM_NORMAL_TEXTURE;

        Shader.SetGlobalTexture(shaderTextureParameter, DataTexture);
        Shader.SetGlobalMatrix(Globals.SHADER_PARAM_LIGHT_MATRIX, lightMatrix);
    }
}
