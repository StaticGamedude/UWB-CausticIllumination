using UnityEngine;

using System.Linq;

/// <summary>
/// Handles the overall scene logic. Reads users input and renders debug spheres and cylinders
/// at positions read from a render texture
/// </summary>
public class World : MonoBehaviour
{
    /// <summary>
    /// The render texture containing the world positions of the verticies of a refractive object that can be seen from a light source
    /// </summary>
    public RenderTexture LightCameraRefractionPositionTexture;

    /// <summary>
    /// The render texture containing the world normals of the verticies of a refractive object that can be seen from a light source
    /// </summary>
    public RenderTexture LightCameraRefractionNormalTexture;

    /// <summary>
    /// The render texture containing the world positions of the verticies of a receiving object that can be seen from a light source
    /// </summary>
    public RenderTexture LightCameraReceivingPositionTexture;

    /// <summary>
    /// The render texture containing the world position of "splatted" points which make up the caustic map
    /// </summary>
    public RenderTexture LightCameraCausticTexture;

    /// <summary>
    /// The render texture containing the color of the specular object
    /// </summary>
    public RenderTexture LightCameraCausticColorTexture;

    /// <summary>
    /// The render texture containing the flux values of the specular object
    /// </summary>
    public RenderTexture LightCameraFluxTexture;

    /// <summary>
    /// The render texture containing the distance values between a specular vertex and splat position
    /// </summary>
    public RenderTexture LightCausticDistanceTexture;

    /// <summary>
    /// The render texture containing the final intensity/caustic information
    /// </summary>
    public RenderTexture LightCameraCausticFinalTexture;

    /// <summary>
    /// The render texture containing the refracted ray directions
    /// </summary>
    public RenderTexture LightCameraRefractionRayTexture;

    /// <summary>
    /// A reference to the diffuse object used for testing/experimenting
    /// </summary>
    public GameObject DiffuseObject;

    /// <summary>
    /// Limits the number of objects created when rendering validation objects in the scene. Only every Xth item will be rendered.
    /// </summary>
    public int Debug_RenderEveryXElement = 20;

    /// <summary>
    /// Determines the size of the position spheres rendered in the scene
    /// </summary>
    public Vector3 Debug_SpherePositionSize = new Vector3(0.01f, 0.01f, 0.01f);

    /// <summary>
    /// Determines the size of the normal cylinders rendered in the scene
    /// </summary>
    public Vector3 Debug_CylinderNormalSize = new Vector3(0.003f, 0.02f, 0.0003f);

    /// <summary>
    /// Sets the refraction index for specular object
    /// </summary>
    public float Debug_RefractionIndex = 1; // Defaulting to 1. Equivalent to the speed of light moving in a vacuum. 

    /// <summary>
    /// Gets/sets a debug flux value. Used primarily for debugging when needing a constant flux value
    /// </summary>
    public float Debug_Flux = 0.5f;

    /// <summary>
    /// Gets/sets the value multiplied the flux values read in the flux texture
    /// </summary>
    public float Flux_Multiplier = 500;

    /// <summary>
    /// Gets/sets the visible surface area of a specular object seen by a caustic camera
    /// </summary>
    public int Debug_NumOfVisiblePixels = 600;

    /// <summary>
    /// A flag which determines whether shadows are rendered
    /// </summary>
    public bool RenderShadows = true;

    /// <summary>
    /// A flag which determines whether the caustic effect is rendered
    /// </summary>
    public bool RenderCaustics = true;

    /// <summary>
    /// Gets/sets the shadow theshold. If a value in the final shadow texture is greater than this value,
    /// we treat that position as being "in a shadow"
    /// </summary>
    public float ShadowThreshold = 0.01f;

    /// <summary>
    /// Gets/sets the size of the Gaussian blur kernel that is applied to the caustic effect
    /// </summary>
    public int CausticBlurKernalSize = 5;

    /// <summary>
    /// Gets/sets the size of the Gassian blur kernel that is applied to the shadow effect
    /// </summary>
    public int ShadowBlurKernelSize = 5;

    /// <summary>
    /// Gets/sets the caustic threshold. If a value in the final caustic texture is greater than this value,
    /// we treat the position as being in the caustic-receiving position 
    /// </summary>
    public float CausticThreshold = 0.1f;

    /// <summary>
    /// Gets/sets the max number light sources supported in the application
    /// </summary>
    private int supportedNumberOfLights = 8;

    /// <summary>
    /// A list of our current light source information
    /// </summary>
    private LightSourceDataProperties[] allLightSourceData;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Assert(LightCameraRefractionPositionTexture != null);
        Debug.Assert(LightCameraRefractionNormalTexture != null);

        LightSource[] foundLightSources = FindObjectsOfType<LightSource>();

        this.allLightSourceData = new LightSourceDataProperties[this.supportedNumberOfLights];
        for(int i = 0; i < this.supportedNumberOfLights; i++)
        {
            if (i >= foundLightSources.Length)
            {
                LightSourceDataProperties dummyDataProperty = new LightSourceDataProperties();
                dummyDataProperty.LightSourceID = -1;
                this.allLightSourceData[i] = dummyDataProperty;
            }
            else
            {
                this.allLightSourceData[i] = foundLightSources[i].dataProperties;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        float[] lightIDs = this.allLightSourceData.Select(data => (float)data.LightSourceID).ToArray();

        if (this.Debug_RefractionIndex < 1)
        {
            this.Debug_RefractionIndex = 1;
        }

        if (this.CausticBlurKernalSize < 1)
        {
            this.CausticBlurKernalSize = 1;
        }

        Shader.SetGlobalFloat("_RefractiveIndex", this.Debug_RefractionIndex);
        Shader.SetGlobalVector("_DiffuseObjectPos", this.DiffuseObject.transform.position);
        Shader.SetGlobalTexture("_CausticTexture", this.LightCameraCausticFinalTexture);
        Shader.SetGlobalTexture("_CausticFluxTexture", this.LightCameraFluxTexture);
        Shader.SetGlobalInt("_NumProjectedVerticies", this.Debug_NumOfVisiblePixels);
        Shader.SetGlobalFloat("_DebugFlux", this.Debug_Flux);
        Shader.SetGlobalFloat("_DebugFluxMultiplier", this.Flux_Multiplier);
        Shader.SetGlobalInt("_RenderShadows", this.RenderShadows ? 1 : 0);
        Shader.SetGlobalInt("_RenderCaustics", this.RenderCaustics ? 1 : 0);
        Shader.SetGlobalFloat("_ShadowThreshold", this.ShadowThreshold);
        Shader.SetGlobalFloat("_CausticThreshold", this.CausticThreshold);
        Shader.SetGlobalInt("_CausticBlurKernalSize", this.CausticBlurKernalSize);
        Shader.SetGlobalInt("_ShadowBlurKernelSize", this.ShadowBlurKernelSize);

        Shader.SetGlobalFloatArray("_LightIDs", lightIDs);
    }
}