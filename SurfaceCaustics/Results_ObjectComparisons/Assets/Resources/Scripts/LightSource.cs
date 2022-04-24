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

    // Start is called before the first frame update
    void Start()
    {
        List<Camera> passOneCameras = this.InitializePassOneCameras();
        List<Camera> passTwoCameras = this.InitializePassTwoCameras();
        List<Camera> passThreeCameras = this.InitializePassThreeCamera();

        this.lightCameras.AddRange(passOneCameras);
        this.lightCameras.AddRange(passTwoCameras);
        this.lightCameras.AddRange(passThreeCameras);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private List<Camera> InitializePassOneCameras()
    {
        GameObject passOneCameraContainer = new GameObject();
        passOneCameraContainer.name = "PassOneCameras";

        Shader refractivePositionShader = Resources.Load<Shader>(Path.Combine(SHADERS_DIRETORY, "PositionShader"));
        Shader refractiveNormalShader = Resources.Load<Shader>(Path.Combine(SHADERS_DIRETORY, "LightNormalShader"));
        Shader receivingPositionShader = Resources.Load<Shader>(Path.Combine(SHADERS_DIRETORY, "ReceivingObjectShader"));
        Camera refractivePositionCamera = this.InstantiateCausticCamera("SpecularPositionsCamera", passOneCameraContainer, this.InitRenderTexture(false), refractivePositionShader, LightCameraType.REFRACTIVE_POSITION, LightCameraVisibilityType.SPECULAR);
        Camera refractiveNormalCamera = this.InstantiateCausticCamera("SpecularNormalsCamera", passOneCameraContainer, this.InitRenderTexture(false), refractiveNormalShader, LightCameraType.REFRACTIVE_NORMAL, LightCameraVisibilityType.SPECULAR);
        Camera receivingPositionCamera = this.InstantiateCausticCamera("ReceivingPositionCamera", passOneCameraContainer, this.InitRenderTexture(false), receivingPositionShader, LightCameraType.RECEIVING_POSITION, LightCameraVisibilityType.RECEIVER);

        passOneCameraContainer.transform.parent = this.transform;
        passOneCameraContainer.transform.localPosition = Vector3.zero;
        passOneCameraContainer.transform.localRotation = Quaternion.identity;
        passOneCameraContainer.transform.localScale = Vector3.one;

        return new List<Camera>() { refractivePositionCamera, refractiveNormalCamera, receivingPositionCamera };
    }

    private List<Camera> InitializePassTwoCameras()
    {
        GameObject passTwoCameraContainer = new GameObject();
        passTwoCameraContainer.name = "PassTwoCameras";

        Shader refractionDistanceShader = Resources.Load<Shader>(Path.Combine(SHADERS_DIRETORY, "CausticDistanceShader"));
        Shader refractionFluxShader = Resources.Load<Shader>(Path.Combine(SHADERS_DIRETORY, "CausticFluxShader"));
        Shader refractionColorShader = Resources.Load<Shader>(Path.Combine(SHADERS_DIRETORY, "DrewCausticColorShader"));
        Camera refractionDistanceCamera = this.InstantiateCausticCamera("SpecularDistanceCamera", passTwoCameraContainer, this.InitRenderTexture(false), refractionDistanceShader, LightCameraType.CAUSTIC_DISTANCE, LightCameraVisibilityType.SPECULAR);
        Camera refractionFluxCamera = this.InstantiateCausticCamera("SpecularFluxCamera", passTwoCameraContainer, this.InitRenderTexture(false), refractionFluxShader, LightCameraType.CAUSTIC_FLUX_2, LightCameraVisibilityType.SPECULAR);
        Camera refractionColorCamera = this.InstantiateCausticCamera("SpecularColorCamera", passTwoCameraContainer, this.InitRenderTexture(true), refractionColorShader, LightCameraType.CAUSTIC_DREW_COLOR, LightCameraVisibilityType.SPECULAR);

        passTwoCameraContainer.transform.parent = this.transform;
        passTwoCameraContainer.transform.localPosition = Vector3.zero;
        passTwoCameraContainer.transform.localRotation = Quaternion.identity;
        passTwoCameraContainer.transform.localScale = Vector3.one;

        return new List<Camera>() { refractionDistanceCamera, refractionFluxCamera, refractionColorCamera };
    }

    private List<Camera> InitializePassThreeCamera()
    {
        GameObject passThreeCameraContainer = new GameObject();
        passThreeCameraContainer.name = "PassThreeCameras";

        Shader refractionFinal = Resources.Load<Shader>(Path.Combine(SHADERS_DIRETORY, "CausticFinalShader"));
        Camera refractionResultCamera = this.InstantiateCausticCamera("SpecularResultCamera", passThreeCameraContainer, this.InitRenderTexture(true), refractionFinal, LightCameraType.CAUSTIC_FINAL_LIGHT_COLOR, LightCameraVisibilityType.SPECULAR);

        passThreeCameraContainer.transform.parent = this.transform;
        passThreeCameraContainer.transform.localPosition = Vector3.zero;
        passThreeCameraContainer.transform.localRotation = Quaternion.identity;
        passThreeCameraContainer.transform.localScale = Vector3.one;


        return new List<Camera>() { refractionResultCamera };
    }

    private RenderTexture InitRenderTexture(bool isColorTexture)
    {
        RenderTexture newTexture = new RenderTexture(1024, 1024, 32);
        newTexture.format = !isColorTexture ? RenderTextureFormat.ARGBFloat : RenderTextureFormat.Default;
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

        cameraObject.transform.parent = parentObj.transform;

        return camera;
    }
}
