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

    private Camera tempCam;

    // Start is called before the first frame update
    void Awake()
    {
        this.dataProperties = new LightSourceDataProperties();
        this.dataProperties.LightSourceID = lightSourceID;

        List<Camera> passOneCameras = this.InitializePassOneCameras(dataProperties);
        List<Camera> passTwoCameras = this.InitializePassTwoCameras(dataProperties);
        List<Camera> passThreeCameras = this.InitializePassThreeCamera(dataProperties);

        this.lightCameras.AddRange(passOneCameras);
        this.lightCameras.AddRange(passTwoCameras);
        this.lightCameras.AddRange(passThreeCameras);

        lightSourceID++;
    }

    private void Start()
    {
        //foreach (Camera lightCam in this.lightCameras)
        //{
        //    //var renderTex = lightCam.targetTexture;

        //    //this.SetLightIDOnShader(refractionFinalShader);
        //}
    }



    // Update is called once per frame
    void Update()
    {
        this.SetLightIDOnShader(tempShader);
        Shader.SetGlobalColor("_DebugLightColor", this.LightColor);
        Shader.SetGlobalFloat("_LightIntensity", this.LightIntensity);
        Shader.SetGlobalTexture($"_LightTexture{this.dataProperties.LightSourceID}", this.dataProperties.FinalColorTexture);
    }

    private List<Camera> InitializePassOneCameras(LightSourceDataProperties dataProperties)
    {
        GameObject passOneCameraContainer = new GameObject();
        passOneCameraContainer.name = "PassOneCameras";

        Shader refractivePositionShader = Resources.Load<Shader>(Path.Combine(SHADERS_DIRETORY, "PositionShader"));
        Shader refractiveNormalShader = Resources.Load<Shader>(Path.Combine(SHADERS_DIRETORY, "LightNormalShader"));
        Shader receivingPositionShader = Resources.Load<Shader>(Path.Combine(SHADERS_DIRETORY, "ReceivingObjectShader"));
        RenderTexture refractivePositionTexture = this.InitRenderTexture(false, "RefractionPositionTexture");
        RenderTexture refractiveNormalTexture = this.InitRenderTexture(false, "RefractionNormalTexture");
        RenderTexture receivingPositionTexture = this.InitRenderTexture(false, "ReceivingPositionTexture");
        
        Camera refractivePositionCamera = this.InstantiateCausticCamera("SpecularPositionsCamera", passOneCameraContainer, refractivePositionTexture, refractivePositionShader, LightCameraType.REFRACTIVE_POSITION, LightCameraVisibilityType.SPECULAR);
        Camera refractiveNormalCamera = this.InstantiateCausticCamera("SpecularNormalsCamera", passOneCameraContainer, refractiveNormalTexture, refractiveNormalShader, LightCameraType.REFRACTIVE_NORMAL, LightCameraVisibilityType.SPECULAR);
        Camera receivingPositionCamera = this.InstantiateCausticCamera("ReceivingPositionCamera", passOneCameraContainer, receivingPositionTexture, receivingPositionShader, LightCameraType.RECEIVING_POSITION, LightCameraVisibilityType.RECEIVER);

        passOneCameraContainer.transform.parent = this.transform;
        passOneCameraContainer.transform.localPosition = Vector3.zero;
        passOneCameraContainer.transform.localRotation = Quaternion.identity;
        passOneCameraContainer.transform.localScale = Vector3.one;

        dataProperties.RefractionPositionTexture = refractivePositionTexture;
        dataProperties.RefractionNormalTexture = refractiveNormalTexture;
        dataProperties.ReceivingPositionTexture = receivingPositionTexture;

        return new List<Camera>() { refractivePositionCamera, refractiveNormalCamera, receivingPositionCamera };
    }

    private List<Camera> InitializePassTwoCameras(LightSourceDataProperties dataProperties)
    {
        GameObject passTwoCameraContainer = new GameObject();
        passTwoCameraContainer.name = "PassTwoCameras";

        //Shader refractionDistanceShader = Resources.Load<Shader>(Path.Combine(SHADERS_DIRETORY, "CausticDistanceShader"));
        Shader refractionFluxShader = Resources.Load<Shader>(Path.Combine(SHADERS_DIRETORY, "CausticFluxShader"));
        Shader refractionColorShader = Resources.Load<Shader>(Path.Combine(SHADERS_DIRETORY, "DrewCausticColorShader"));
        //RenderTexture refractionDistanceTexture = this.InitRenderTexture(false, "RefractionDistanceTexture");
        RenderTexture refractionFluxTexture = this.InitRenderTexture(false, "RefractionFluxTexture");
        RenderTexture refractionColorTexture = this.InitRenderTexture(true, "RefractionColorTexture");
        //Camera refractionDistanceCamera = this.InstantiateCausticCamera("SpecularDistanceCamera", passTwoCameraContainer, refractionDistanceTexture, refractionDistanceShader, LightCameraType.CAUSTIC_DISTANCE, LightCameraVisibilityType.SPECULAR);
        Camera refractionFluxCamera = this.InstantiateCausticCamera("SpecularFluxCamera", passTwoCameraContainer, refractionFluxTexture, refractionFluxShader, LightCameraType.CAUSTIC_FLUX_2, LightCameraVisibilityType.SPECULAR);
        Camera refractionColorCamera = this.InstantiateCausticCamera("SpecularColorCamera", passTwoCameraContainer, refractionColorTexture, refractionColorShader, LightCameraType.CAUSTIC_DREW_COLOR, LightCameraVisibilityType.SPECULAR);

        passTwoCameraContainer.transform.parent = this.transform;
        passTwoCameraContainer.transform.localPosition = Vector3.zero;
        passTwoCameraContainer.transform.localRotation = Quaternion.identity;
        passTwoCameraContainer.transform.localScale = Vector3.one;

        //dataProperties.RefractionDistanceTexture = refractionDistanceTexture;
        dataProperties.RefractionFluxTexture = refractionFluxTexture;
        dataProperties.RefractionColorTexture = refractionColorTexture;

        return new List<Camera>() { /*refractionDistanceCamera,*/ refractionFluxCamera, refractionColorCamera };
    }

    private List<Camera> InitializePassThreeCamera(LightSourceDataProperties dataProperties)
    {
        GameObject passThreeCameraContainer = new GameObject();
        passThreeCameraContainer.name = "PassThreeCameras";

        Shader refractionFinalShader = Resources.Load<Shader>(Path.Combine(SHADERS_DIRETORY, "CausticFinalShader"));
        RenderTexture refractionFinalTexture = this.InitRenderTexture(true, "RefractionFinalTexture");
        Camera refractionResultCamera = this.InstantiateCausticCamera("SpecularResultCamera", passThreeCameraContainer, refractionFinalTexture, refractionFinalShader, LightCameraType.CAUSTIC_FINAL_LIGHT_COLOR, LightCameraVisibilityType.SPECULAR);

        passThreeCameraContainer.transform.parent = this.transform;
        passThreeCameraContainer.transform.localPosition = Vector3.zero;
        passThreeCameraContainer.transform.localRotation = Quaternion.identity;
        passThreeCameraContainer.transform.localScale = Vector3.one;

        dataProperties.FinalColorTexture = refractionFinalTexture;

        this.tempShader = refractionFinalShader;

        return new List<Camera>() { refractionResultCamera };
    }

    private RenderTexture InitRenderTexture(bool isColorTexture, string name)
    {
        RenderTexture newTexture = new RenderTexture(1024, 1024, 32);
        newTexture.name = name;
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

        if (lightCamType == LightCameraType.CAUSTIC_FINAL_LIGHT_COLOR)
        {
            lightCamLogic.finalLightShaderParameter = $"_LightTexture{this.dataProperties.LightSourceID}";
        }

        cameraObject.transform.parent = parentObj.transform;

        return camera;
    }

    private void SetLightIDOnShader(Shader shader)
    {
        Material tempMat = new Material(shader);
        tempMat.SetInt("_LightID", this.dataProperties.LightSourceID);
    }
}
