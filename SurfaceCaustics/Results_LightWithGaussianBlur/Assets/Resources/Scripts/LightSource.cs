using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LightSource : MonoBehaviour
{
    private const string SHADERS_DIRETORY = @"Shaders";

    private const string SCRIPTS_DIRECTORY = @"Scripts";

    private const string SPECULAR_SHADER_TAG = "Specular";

    private const string RECEIVER_SHADER_TAG = "Receiver";

    private List<Camera> lightCameras = new List<Camera>();

    public float LightIntensity = 1;

    public Color LightColor = Color.white;

    public LightSourceDataProperties dataProperties;

    private static int lightSourceID = 0;

    private Shader tempShader;

    // Start is called before the first frame update
    void Awake()
    {
        this.dataProperties = new LightSourceDataProperties();
        this.dataProperties.LightSourceID = lightSourceID;

        List<Camera> passOneCameras = this.InitializePassOneCameras(dataProperties);
        List<Camera> passTwoCameras = this.InitializePassTwoCameras(dataProperties);
        List<Camera> passThreeCameras = this.InitializePassThreeCamera(dataProperties);
        List<Camera> passFourCameras = this.InitializePassFourCamera(dataProperties);

        this.lightCameras.AddRange(passOneCameras);
        this.lightCameras.AddRange(passTwoCameras);
        this.lightCameras.AddRange(passThreeCameras);
        this.lightCameras.AddRange(passFourCameras);

        lightSourceID++;
    }

    // Update is called once per frame
    void Update()
    {
        Shader.SetGlobalColor($"_DebugLightColor_{this.dataProperties.LightSourceID}", this.LightColor);
        Shader.SetGlobalFloat($"_LightIntensity_{this.dataProperties.LightSourceID}", this.LightIntensity);
    }

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

    private List<Camera> InitializePassTwoCameras(LightSourceDataProperties dataProperties)
    {
        GameObject passTwoCameraContainer = new GameObject();
        passTwoCameraContainer.name = "PassTwoCameras";

        Shader refractionFluxShader = Resources.Load<Shader>(Path.Combine(SHADERS_DIRETORY, $"CausticFluxShader_{this.dataProperties.LightSourceID}"));
        Shader refractionColorShader = Resources.Load<Shader>(Path.Combine(SHADERS_DIRETORY, $"DrewCausticColorShader_{this.dataProperties.LightSourceID}"));
        Shader shadowShader = Resources.Load<Shader>(Path.Combine(SHADERS_DIRETORY, $"CausticShadowShader_{this.dataProperties.LightSourceID}"));
        RenderTexture refractionFluxTexture = this.InitRenderTexture(false, "RefractionFluxTexture");
        RenderTexture refractionColorTexture = this.InitRenderTexture(true, "RefractionColorTexture");
        RenderTexture refractionShadowTexture = this.InitRenderTexture(false, "RefractionShadowTexture");
        Camera refractionFluxCamera = this.InstantiateCausticCamera("SpecularFluxCamera", passTwoCameraContainer, refractionFluxTexture, refractionFluxShader, LightCameraType.CAUSTIC_FLUX_2, LightCameraVisibilityType.SPECULAR);
        Camera refractionColorCamera = this.InstantiateCausticCamera("SpecularColorCamera", passTwoCameraContainer, refractionColorTexture, refractionColorShader, LightCameraType.CAUSTIC_DREW_COLOR, LightCameraVisibilityType.SPECULAR);
        Camera refractionShadowCamera = this.InstantiateCausticCamera("SpecularShadowCamera", passTwoCameraContainer, refractionShadowTexture, shadowShader, LightCameraType.SHADOW, LightCameraVisibilityType.SPECULAR);

        passTwoCameraContainer.transform.parent = this.transform;
        passTwoCameraContainer.transform.localPosition = Vector3.zero;
        passTwoCameraContainer.transform.localRotation = Quaternion.identity;
        passTwoCameraContainer.transform.localScale = Vector3.one;

        dataProperties.RefractionFluxTexture = refractionFluxTexture;
        dataProperties.RefractionColorTexture = refractionColorTexture;

        return new List<Camera>() { refractionFluxCamera, refractionColorCamera, refractionShadowCamera };
    }

    private List<Camera> InitializePassThreeCamera(LightSourceDataProperties dataProperties)
    {
        GameObject passThreeCameraContainer = new GameObject();
        passThreeCameraContainer.name = "PassThreeCameras";

        Shader refractionFinalShader = Resources.Load<Shader>(Path.Combine(SHADERS_DIRETORY, $"CausticFinalShader_{this.dataProperties.LightSourceID}"));
        Shader shadowFinalShader = Resources.Load<Shader>(Path.Combine(SHADERS_DIRETORY, $"ShadowFinalShader_{this.dataProperties.LightSourceID}"));
        RenderTexture refractionFinalTexture = this.InitRenderTexture(true, "RefractionFinalTexture");
        RenderTexture shadowFinalTexture = this.InitRenderTexture(true, "ShadowFinalTexture");
        Camera refractionResultCamera = this.InstantiateCausticCamera("SpecularResultCamera", passThreeCameraContainer, refractionFinalTexture, refractionFinalShader, LightCameraType.CAUSTIC_FINAL_LIGHT_COLOR, LightCameraVisibilityType.SPECULAR);
        Camera shadowFinalCamera = this.InstantiateCausticCamera("ShadowFinalCamera", passThreeCameraContainer, shadowFinalTexture, shadowFinalShader, LightCameraType.SHADOW_FINAL, LightCameraVisibilityType.SPECULAR);

        passThreeCameraContainer.transform.parent = this.transform;
        passThreeCameraContainer.transform.localPosition = Vector3.zero;
        passThreeCameraContainer.transform.localRotation = Quaternion.identity;
        passThreeCameraContainer.transform.localScale = Vector3.one;

        dataProperties.FinalColorTexture = refractionFinalTexture;

        this.tempShader = refractionFinalShader;

        return new List<Camera>() { refractionResultCamera, shadowFinalCamera };
    }

    private List<Camera> InitializePassFourCamera(LightSourceDataProperties dataProperties)
    {
        GameObject passFourCameraContainer = new GameObject();
        passFourCameraContainer.name = "PassFourCameras";

        Shader gaussianShader = Resources.Load<Shader>(Path.Combine(SHADERS_DIRETORY, $"GaussianShader_{this.dataProperties.LightSourceID}"));
        RenderTexture gaussianCausticTexture = this.InitRenderTexture(true, "RefractionFinalTexture");
        Camera gaussianCausticResultCamera = this.InstantiateCausticCamera("GaussianCausticCamera", passFourCameraContainer, gaussianCausticTexture, gaussianShader, LightCameraType.GAUSSIAN, LightCameraVisibilityType.SPECULAR);

        passFourCameraContainer.transform.parent = this.transform;
        passFourCameraContainer.transform.localPosition = Vector3.zero;
        passFourCameraContainer.transform.localRotation = Quaternion.identity;
        passFourCameraContainer.transform.localScale = Vector3.one;

        return new List<Camera>() { gaussianCausticResultCamera };
    }

    private RenderTexture InitRenderTexture(bool isColorTexture, string name)
    {
        RenderTexture newTexture = new RenderTexture(1024, 1024, 32);
        newTexture.name = name;
        newTexture.format = !isColorTexture ? RenderTextureFormat.ARGBFloat : RenderTextureFormat.Default;
        newTexture.antiAliasing = 8;
        newTexture.autoGenerateMips = false;
        newTexture.useDynamicScale = false;
        newTexture.useMipMap = false;
        return newTexture;
    }

    private Camera InstantiateCausticCamera(string cameraName, GameObject parentObj, RenderTexture targetRenderTexture, Shader replacementShader, LightCameraType lightCamType, LightCameraVisibilityType visibilityType)
    {
        GameObject cameraObject = new GameObject(cameraName);
        Camera camera = cameraObject.AddComponent<Camera>();
        LightCamera lightCamLogic = cameraObject.AddComponent<LightCamera>();

        camera.targetTexture = targetRenderTexture;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Color.black;

        lightCamLogic.SetTextureSize = false;
        lightCamLogic.SpecularObjectShader = replacementShader;
        lightCamLogic.DataTexture = targetRenderTexture;
        lightCamLogic.LightCamType = lightCamType;
        lightCamLogic.LightCameraVisibilityType = visibilityType;
        lightCamLogic.LightSourceID = this.dataProperties.LightSourceID;

        if (lightCamType == LightCameraType.CAUSTIC_FINAL_LIGHT_COLOR)
        {
            lightCamLogic.finalLightShaderParameter = $"_LightTexture{this.dataProperties.LightSourceID}";
        }

        cameraObject.transform.parent = parentObj.transform;

        return camera;
    }
}
