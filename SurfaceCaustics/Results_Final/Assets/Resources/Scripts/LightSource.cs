using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Handles the logic for a light source in the scene. Reponsible for generating caustic cameras which are responsible for capturing
/// data used to render caustic effects. Each light source will have an associated ID with it which helps the shaders differentiate the
/// available light source data
/// </summary>
public class LightSource : MonoBehaviour
{
    /// <summary>
    /// Expected path in the Resources directory that contains all of the caustic shaders
    /// </summary>
    private const string SHADERS_DIRETORY = @"Shaders";

    /// <summary>
    /// A list of caustic cameras attached to the the light source object
    /// </summary>
    private List<Camera> lightCameras = new List<Camera>();

    /// <summary>
    /// Unique ID for the light source
    /// </summary>
    private static int lightSourceID = 0;

    /// <summary>
    /// Gets/Sets the intensity of the light source. Value can be altered by the user in the Unity editor
    /// </summary>
    public float LightIntensity = 1;

    /// <summary>
    /// Gets the desired color of the light source. Value can be altered by the user in the Unity editor
    /// </summary>
    public Color LightColor = Color.white;

    /// <summary>
    /// Sets the resolution size of the caustic textures
    /// </summary>
    public int RenderTextureSize = 1024;

    public bool IsDirectionalLight = false;

    /// <summary>
    /// Contains the caustic data associated with the light source
    /// </summary>
    public LightSourceDataProperties dataProperties;

    /// <summary>
    /// When this object "wakes up", initialize our caustic cameras and add them to the light source. This method also assigns
    /// the light source and makes sure to iterate the light source counter.
    /// </summary>
    void Awake()
    {
        this.dataProperties = new LightSourceDataProperties();
        this.dataProperties.LightSourceID = lightSourceID;

        List<Camera> passOneCameras = this.InitializePassOneCameras(dataProperties);
        List<Camera> passTwoCameras = this.InitializePassTwoCameras(dataProperties);
        List<Camera> passThreeCameras = this.InitializePassThreeCamera(dataProperties);
        //List<Camera> passFourCameras = this.InitializePassFourCamera(dataProperties);

        this.lightCameras.AddRange(passOneCameras);
        this.lightCameras.AddRange(passTwoCameras);
        this.lightCameras.AddRange(passThreeCameras);
        //this.lightCameras.AddRange(passFourCameras);

        lightSourceID++;
    }

    /// <summary>
    /// Upon update, tell all the shaders what this light source's intensity and color are.
    /// </summary>
    void Update()
    {
        Shader.SetGlobalColor($"_DebugLightColor_{this.dataProperties.LightSourceID}", this.LightColor);
        Shader.SetGlobalFloat($"_LightIntensity_{this.dataProperties.LightSourceID}", this.LightIntensity);
        Shader.SetGlobalInt($"_LightIsDirectional_{this.dataProperties.LightSourceID}", this.IsDirectionalLight ? 1 : 0);
    }

    /// <summary>
    /// Initializes the caustic cameras needed to capture the pass 1 data. For pass 1, only the receiving object position texture is needed.
    /// </summary>
    /// <param name="dataProperties">Information about the current light source</param>
    /// <returns>List of cameras added to the scene to support capturing pass 1 data</returns>
    private List<Camera> InitializePassOneCameras(LightSourceDataProperties dataProperties)
    {
        GameObject passOneCameraContainer = new GameObject();
        passOneCameraContainer.name = "PassOneCameras";

        Shader receivingPositionShader = Resources.Load<Shader>(Path.Combine(SHADERS_DIRETORY, "ReceivingObjectShader"));
        RenderTexture receivingPositionTexture = this.InitRenderTexture(false, "ReceivingPositionTexture");
        Camera receivingPositionCamera = this.InstantiateCausticCamera("ReceivingPositionCamera", passOneCameraContainer, receivingPositionTexture, receivingPositionShader, LightCameraType.RECEIVING_POSITION, LightCameraVisibilityType.RECEIVER);

        passOneCameraContainer.transform.parent = this.transform;
        passOneCameraContainer.transform.localPosition = Vector3.zero;
        passOneCameraContainer.transform.localRotation = Quaternion.identity;
        passOneCameraContainer.transform.localScale = Vector3.one;

        dataProperties.ReceivingPositionTexture = receivingPositionTexture;

        return new List<Camera>() { receivingPositionCamera };
    }

    /// <summary>
    /// Initializes the caustic cameras needed to capture the pass 2 data. The second pass is responsible for capturing
    /// our flux values (from the specular object), the color values (from the specular object), and the shadow values (from the specular object)
    /// </summary>
    /// <param name="dataProperties">Information about the current light source</param>
    /// <returns>List of cameras added to the scene to support capturing pass 2 data</returns>
    private List<Camera> InitializePassTwoCameras(LightSourceDataProperties dataProperties)
    {
        GameObject passTwoCameraContainer = new GameObject();
        passTwoCameraContainer.name = "PassTwoCameras";

        Shader refractionFluxShader = Resources.Load<Shader>(Path.Combine(SHADERS_DIRETORY, $"CausticFluxShader_{this.dataProperties.LightSourceID}"));
        Shader refractionColorShader = Resources.Load<Shader>(Path.Combine(SHADERS_DIRETORY, $"DrewCausticColorShader_{this.dataProperties.LightSourceID}"));
        Shader shadowShader = Resources.Load<Shader>(Path.Combine(SHADERS_DIRETORY, $"CausticShadowShader_{this.dataProperties.LightSourceID}"));
        Shader debugSplatPosShader = Resources.Load<Shader>(Path.Combine(SHADERS_DIRETORY, $"CausticSplatPositionShader_{this.dataProperties.LightSourceID}"));
        RenderTexture refractionFluxTexture = this.InitRenderTexture(false, "RefractionFluxTexture");
        RenderTexture refractionColorTexture = this.InitRenderTexture(false, "RefractionColorTexture");
        RenderTexture refractionShadowTexture = this.InitRenderTexture(false, "RefractionShadowTexture");
        RenderTexture debugSplatTexture = this.InitRenderTexture(false, "SplatPosTexture");
        Camera refractionFluxCamera = this.InstantiateCausticCamera("SpecularFluxCamera", passTwoCameraContainer, refractionFluxTexture, refractionFluxShader, LightCameraType.CAUSTIC_FLUX_2, LightCameraVisibilityType.SPECULAR);
        Camera refractionColorCamera = this.InstantiateCausticCamera("SpecularColorCamera", passTwoCameraContainer, refractionColorTexture, refractionColorShader, LightCameraType.CAUSTIC_DREW_COLOR, LightCameraVisibilityType.SPECULAR);
        Camera refractionShadowCamera = this.InstantiateCausticCamera("SpecularShadowCamera", passTwoCameraContainer, refractionShadowTexture, shadowShader, LightCameraType.SHADOW, LightCameraVisibilityType.SPECULAR);
        Camera debugSplatCamera = this.InstantiateCausticCamera("DebugSplatPosCamera", passTwoCameraContainer, debugSplatTexture, debugSplatPosShader, LightCameraType.DEBUG, LightCameraVisibilityType.SPECULAR);

        passTwoCameraContainer.transform.parent = this.transform;
        passTwoCameraContainer.transform.localPosition = Vector3.zero;
        passTwoCameraContainer.transform.localRotation = Quaternion.identity;
        passTwoCameraContainer.transform.localScale = Vector3.one;

        dataProperties.RefractionFluxTexture = refractionFluxTexture;
        dataProperties.RefractionColorTexture = refractionColorTexture;
        dataProperties.DebugSplatPosTexture = debugSplatTexture;

        return new List<Camera>() { refractionFluxCamera, refractionColorCamera, refractionShadowCamera, debugSplatCamera };
    }

    /// <summary>
    /// Initializes the caustic cameras needed to capture the pass 3 data. The thrid pass contains the final color caustic effect and final shadow result
    /// </summary>
    /// <param name="dataProperties">Information about the current light source</param>
    /// <returns>List of cameras added to the scene to support capturing pass 3 data</returns>
    private List<Camera> InitializePassThreeCamera(LightSourceDataProperties dataProperties)
    {
        GameObject passThreeCameraContainer = new GameObject();
        passThreeCameraContainer.name = "PassThreeCameras";

        Shader refractionFinalShader = Resources.Load<Shader>(Path.Combine(SHADERS_DIRETORY, $"CausticFinalShader_{this.dataProperties.LightSourceID}"));
        Shader shadowFinalShader = Resources.Load<Shader>(Path.Combine(SHADERS_DIRETORY, $"ShadowFinalShader_{this.dataProperties.LightSourceID}"));
        RenderTexture refractionFinalTexture = this.InitRenderTexture(false, "RefractionFinalTexture");
        RenderTexture shadowFinalTexture = this.InitRenderTexture(false, "ShadowFinalTexture");
        Camera refractionResultCamera = this.InstantiateCausticCamera("SpecularResultCamera", passThreeCameraContainer, refractionFinalTexture, refractionFinalShader, LightCameraType.CAUSTIC_FINAL_LIGHT_COLOR, LightCameraVisibilityType.SPECULAR);
        Camera shadowFinalCamera = this.InstantiateCausticCamera("ShadowFinalCamera", passThreeCameraContainer, shadowFinalTexture, shadowFinalShader, LightCameraType.SHADOW_FINAL, LightCameraVisibilityType.SPECULAR);

        passThreeCameraContainer.transform.parent = this.transform;
        passThreeCameraContainer.transform.localPosition = Vector3.zero;
        passThreeCameraContainer.transform.localRotation = Quaternion.identity;
        passThreeCameraContainer.transform.localScale = Vector3.one;

        dataProperties.FinalColorTexture = refractionFinalTexture;

        return new List<Camera>() { refractionResultCamera, shadowFinalCamera };
    }

    /// <summary>
    /// Initialize a render texture which will act as the output for a caustic camera.
    /// </summary>
    /// <param name="isColorTexture">A flag which determines if the render texture is meant to capture color data</param>
    /// <param name="name">Name of the render texture (used mostly for investigation)</param>
    /// <returns>The newly generated render texture</returns>
    private RenderTexture InitRenderTexture(bool isColorTexture, string name)
    {
        RenderTexture newTexture = new RenderTexture(this.RenderTextureSize, this.RenderTextureSize, 32);
        newTexture.name = name;
        newTexture.format = !isColorTexture ? RenderTextureFormat.ARGBFloat : RenderTextureFormat.Default;
        newTexture.antiAliasing = 8;
        newTexture.autoGenerateMips = false;
        newTexture.useDynamicScale = false;
        newTexture.useMipMap = false;
        return newTexture;
    }

    /// <summary>
    /// Initializes a new camera which captures information used to generate a caustic effect
    /// </summary>
    /// <param name="cameraName">Name to assign to the camera object (mostly used for debugging)</param>
    /// <param name="parentObj">Object in which the created camera should be added to</param>
    /// <param name="targetRenderTexture">The target texture that the created camera should use</param>
    /// <param name="replacementShader">The replacement shader that the created camera should use</param>
    /// <param name="lightCamType">The camera type (i.e. flux, color, etc.)</param>
    /// <param name="visibilityType">A flag which indicates that the light camera should specular objects or receiving objects</param>
    /// <returns>The created camera added to the scene</returns>
    private Camera InstantiateCausticCamera(string cameraName, GameObject parentObj, RenderTexture targetRenderTexture, Shader replacementShader, LightCameraType lightCamType, LightCameraVisibilityType visibilityType)
    {
        GameObject cameraObject = new GameObject(cameraName);
        Camera camera = cameraObject.AddComponent<Camera>();
        LightCamera lightCamLogic = cameraObject.AddComponent<LightCamera>();

        camera.targetTexture = targetRenderTexture;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0, 0, 0, 0);

        lightCamLogic.SetTextureSize = false;
        lightCamLogic.SpecularObjectShader = replacementShader;
        lightCamLogic.DataTexture = targetRenderTexture;
        lightCamLogic.LightCamType = lightCamType;
        lightCamLogic.LightCameraVisibilityType = visibilityType;
        lightCamLogic.LightSourceID = this.dataProperties.LightSourceID;
        lightCamLogic.ParentLightSource = this;

        cameraObject.transform.parent = parentObj.transform;

        return camera;
    }
}