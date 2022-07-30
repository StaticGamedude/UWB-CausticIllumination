using UnityEngine;

/// <summary>
/// Contains the logical operations to support the functionality of a single caustic camera which acts in place of a light source.
/// </summary>
public class LightCamera : MonoBehaviour
{
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
    public RenderTexture DataTexture;

    /// <summary>
    /// Represents the type of data that this camera is meant to capture
    /// </summary>
    public LightCameraType LightCamType;

    /// <summary>
    /// Represents the type of objects that this camera can see
    /// </summary>
    public LightCameraVisibilityType LightCameraVisibilityType;

    /// <summary>
    /// A flag used to indicate whether the output texture size should be set upon initialization. Mostly used
    /// for debugging when adding light camera's from the debugging
    /// </summary>
    public bool SetTextureSize = true;

    /// <summary>
    /// Gets/sets the light source ID that this camera is capturing data for
    /// </summary>
    public int LightSourceID;

    /// <summary>
    /// Gets/sets the light source object that this camera is capturing data for. The expectation is that
    /// this light camera object is nested under the light source object
    /// </summary>
    public LightSource ParentLightSource;

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

        if (this.SetTextureSize)
        {
            this.DataTexture.width = 256;
            this.DataTexture.height = 256;
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
    /// Every frame, make sure our camera position/rotation match the orientation and position of our light source. Also,
    /// the shader parameters for the light camera
    /// </summary>
    private void Update()
    {
        if (this.ParentLightSource != null)
        {
            Transform parentLightSourceTransform = this.ParentLightSource.transform;
            this.lightCamera.transform.position = parentLightSourceTransform.position;
            this.lightCamera.transform.rotation = parentLightSourceTransform.rotation;

            Matrix4x4 lightMatrix = Globals.BIAS * this.lightCamera.projectionMatrix * lightCamera.worldToCameraMatrix;
            Shader.SetGlobalMatrix($"_LightMatrix_{this.LightSourceID}", lightMatrix);
            Shader.SetGlobalMatrix($"_LightCamMatrix_{this.LightSourceID}", this.lightCamera.worldToCameraMatrix);
            Shader.SetGlobalMatrix($"_LightViewProjectionMatrix_{this.LightSourceID}", this.lightCamera.projectionMatrix * lightCamera.worldToCameraMatrix);
            Shader.SetGlobalFloat($"_LightCam_Far_{this.LightSourceID}", 1.0f / lightCamera.farClipPlane);
            Shader.SetGlobalVector($"_LightWorldPosition_{this.LightSourceID}", lightCamera.transform.position);
            Shader.SetGlobalVector($"_LightCam_Forward_{this.LightSourceID}", lightCamera.transform.forward);
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
                shaderTextureParameter = "_SpecularPosTexture";
                break;
            case LightCameraType.REFRACTIVE_NORMAL:
                shaderTextureParameter = "_SpecularNormTexture";
                break;
            case LightCameraType.RECEIVING_POSITION:
                shaderTextureParameter = "_ReceivingPosTexture";
                break;
            case LightCameraType.CAUSTIC:
                shaderTextureParameter = "_CausticMapTexture";
                break;
            case LightCameraType.CAUSTIC_REFRACTION_RAY:
                shaderTextureParameter = "_CausticRefractionRayTexture";
                break;
            case LightCameraType.CAUSTIC_COLOR:
                shaderTextureParameter = "_CausticColorMapTexture";
                break;
            case LightCameraType.CAUSTIC_FLUX:
                // I have no idea why but...I can't get this option to work like originally expected...
                // Ended up creating a second enum (CAUSTIC_FLUX_2) to represent the caustic flux shader. It uses a different
                // render texure and saves the texture under a different shader parameter
                shaderTextureParameter = "_CausticFluxTexture";
                break;
            case LightCameraType.CAUSTIC_FLUX_2:
                shaderTextureParameter = "_DrewTest";
                break;
            case LightCameraType.CAUSTIC_DISTANCE:
                shaderTextureParameter = "_CausticDistanceTexture";
                break;
            case LightCameraType.CAUSTIC_DREW_COLOR:
                // Similar to the comment above - I have no idea why but...I can't get this option to work like originally 
                // expected...Ended up creating a second enum (CAUSTIC_DREW_COLOR) to represent the caustic color shader. 
                // It uses a different render texure and saves the texture under a different shader parameter
                shaderTextureParameter = "_DrewCausticColor";
                break;
            case LightCameraType.CAUSTIC_FINAL_LIGHT_COLOR:
                shaderTextureParameter = "_FinalLightColorTexture";
                break;
            case LightCameraType.SHADOW:
                shaderTextureParameter = "_CausticShadowTexture";
                break;
            case LightCameraType.SHADOW_FINAL:
                shaderTextureParameter = "_ShadowFinalTexture";
                break;
            case LightCameraType.GAUSSIAN:
                shaderTextureParameter = "_CausticGaussianTexture";
                break;
            case LightCameraType.DEBUG:
                shaderTextureParameter = "_CausticDebug";
                break;
        }

        Shader.SetGlobalTexture($"{shaderTextureParameter}_{this.LightSourceID}", DataTexture);
    }
}