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

    /// <summary>
    /// Represents the type of objects that this camera can see
    /// </summary>
    public LightCameraVisibilityType LightCameraVisibilityType;

    public bool SetTextureSize = true;

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

        //this.lightCamera.fieldOfView = 5;
        if (this.SetTextureSize)
        {
            this.DataTexture.width = 1024; //256
            this.DataTexture.height = 1024; //256
        }
        

        switch (this.LightCameraVisibilityType)
        {
            case LightCameraVisibilityType.SPECULAR:
                this.lightCamera.SetReplacementShader(SpecularObjectShader, Globals.SPECULAR_OBJECT_SHADER_TAG);
                break;
            case LightCameraVisibilityType.RECEIVER:
                this.lightCamera.SetReplacementShader(SpecularObjectShader, Globals.RECEIVING_OBJECT_SHADER_TAG);
                break;
        }
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
            case LightCameraType.CAUSTIC_REFRACTION_RAY:
                shaderTextureParameter = Globals.SHADER_PARAM_CAUSTIC_REFRACTION_RAY_TEXTURE;
                break;
            case LightCameraType.CAUSTIC_COLOR:
                shaderTextureParameter = Globals.SHADER_PARAM_CAUSTIC_COLOR_MAP_TEXTURE;
                break;
            case LightCameraType.CAUSTIC_FLUX:
                // I have no idea why but...I can't get this option to work like originally expected...
                // Ended up creating a second enum (CAUSTIC_FLUX_2) to represent the caustic flux shader. It uses a different
                // render texure and saves the texture under a different shader parameter
                shaderTextureParameter = Globals.SHADER_PARAM_CAUSTIC_FLUX_TEXTURE;
                break;
            case LightCameraType.CAUSTIC_FLUX_2:
                shaderTextureParameter = "_DrewTest";
                break;
            case LightCameraType.CAUSTIC_DISTANCE:
                shaderTextureParameter = Globals.SHADER_PARAM_CAUSTIC_DISTANCE_TEXTURE;
                break;
            case LightCameraType.CAUSTIC_DREW_COLOR:
                shaderTextureParameter = "_DrewCausticColor";
                break;
            case LightCameraType.CAUSTIC_FINAL_LIGHT_COLOR:
                shaderTextureParameter = "_FinalLightColorTexture";
                break;
        }

        Shader.SetGlobalTexture(shaderTextureParameter, DataTexture);
        Shader.SetGlobalMatrix(Globals.SHADER_PARAM_LIGHT_MATRIX, lightMatrix);
        Shader.SetGlobalMatrix(Globals.SHADER_PARAM_LIGHT_CAMERA_MATRIX, this.lightCamera.worldToCameraMatrix);
        Shader.SetGlobalMatrix(Globals.SHADER_PARAM_LIGHT_VIEW_PROJECTION_MATRIX, this.lightCamera.projectionMatrix * lightCamera.worldToCameraMatrix);
        Shader.SetGlobalFloat(Globals.SHADER_PARAM_LIGHT_CAMERA_FAR, 1.0f / lightCamera.farClipPlane);
        Shader.SetGlobalVector(Globals.SHADER_PARAM_LIGHT_WORLD_POS, lightCamera.transform.position);
    }
}
