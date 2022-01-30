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
    /// Represents the type of data that this camera is meant to capture
    /// </summary>
    public LightCameraType LightCamType;

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

        string replacementShaderTag = 
            this.LightCamType == LightCameraType.RECEIVING_POSITION || this.LightCamType == LightCameraType.OTHER
            ? Globals.RECEIVING_OBJECT_SHADER_TAG : Globals.SPECULAR_OBJECT_SHADER_TAG;

        this.lightCamera.SetReplacementShader(SpecularObjectShader, replacementShaderTag);
    }

    /// <summary>
    /// Create the light matrix which will help us transform vertex positions/normals into the light's point of view
    /// </summary>
    private void OnPostRender()
    {
        Matrix4x4 lightMatrix = Globals.BIAS * this.lightCamera.projectionMatrix * lightCamera.worldToCameraMatrix;
        string shaderTextureParameter = string.Empty;
        switch(this.LightCamType)
        {
            case LightCameraType.REFRACTIVE_POSITION:
                shaderTextureParameter = Globals.SHADER_PARAM_REFRACTION_POSITION_TEXTURE;
                break;
            case LightCameraType.REFRACTIVE_NORMAL:
                shaderTextureParameter = Globals.SHADER_PARAM_REFRACTION_NORMAL_TEXTURE;
                break;
            case LightCameraType.RECEIVING_POSITION:
                shaderTextureParameter = Globals.SHADER_PARAM_RECEIVING_POSITION_TEXTURE;
                break;
            case LightCameraType.CAUSTIC:
                shaderTextureParameter = Globals.SHADER_PARAM_CAUSTIC_MAP_TEXTURE;
                break;
        }

        Shader.SetGlobalTexture(shaderTextureParameter, DataTexture);
        Shader.SetGlobalMatrix(Globals.SHADER_PARAM_LIGHT_MATRIX, lightMatrix);
        Shader.SetGlobalMatrix(Globals.SHADER_PARAM_LIGHT_CAMERA_MATRIX, this.lightCamera.worldToCameraMatrix);
        Shader.SetGlobalMatrix(Globals.SHADER_PARAM_LIGHT_VIEW_PROJECTION_MATRIX, this.lightCamera.projectionMatrix * lightCamera.worldToCameraMatrix);
        Shader.SetGlobalFloat(Globals.SHADER_PARAM_LIGHT_CAMERA_FAR, 1.0f / lightCamera.farClipPlane);        
    }
}
