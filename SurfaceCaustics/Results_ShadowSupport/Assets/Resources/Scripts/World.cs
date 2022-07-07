using UnityEngine;

using System.Linq;
using System.Collections.Generic;

/// <summary>
/// Handles the overall scene logic. Reads users input and renders debug spheres and cylinders
/// at positions read from a render texture
/// </summary>
public class World : MonoBehaviour
{
    /// <summary>
    /// Gets/sets the max number light sources supported in the application
    /// </summary>
    private int supportedNumberOfLights = 8;

    /// <summary>
    /// A list of our current light source information
    /// </summary>
    private LightSourceDataProperties[] allLightSourceData;

    private List<GameObject> debugObjects = new List<GameObject>();

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

    public int DebugCameraID = 0;

    public int TextureToRender = 0;

    public int DebugDensity = 20;

    // Start is called before the first frame update
    void Start()
    {
        //Debug.Assert(LightCameraRefractionPositionTexture != null);
        //Debug.Assert(LightCameraRefractionNormalTexture != null);

        List<LightSource> foundLightSources = FindObjectsOfType<LightSource>().ToList();
        this.allLightSourceData = foundLightSources.Select(lightSource => lightSource.dataProperties).ToArray();
    }

    // Update is called once per frame
    void Update()
    {
        float[] lightIDs = new float[this.supportedNumberOfLights];
        for(int i = 0; i < lightIDs.Length; i++)
        {
            lightIDs[i] = -1;
        }

        for(int i = 0; i < this.allLightSourceData.Length; i++)
        {
            lightIDs[i] = 1;
        }

        if (this.Debug_RefractionIndex < 1)
        {
            this.Debug_RefractionIndex = 1;
        }

        if (this.CausticBlurKernalSize < 1)
        {
            this.CausticBlurKernalSize = 1;
        }

        Shader.SetGlobalFloat("_RefractiveIndex", this.Debug_RefractionIndex);
        //Shader.SetGlobalVector("_DiffuseObjectPos", this.DiffuseObject.transform.position);
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

        Shader.SetGlobalFloatArray("_AllLightIds", lightIDs);

        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.R))
        {
            this.debugObjects.ForEach(o => Destroy(o));
            this.debugObjects.Clear();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            this.RenderDebugSpheres();
        }
    }

    private void RenderDebugSpheres()
    {
        LightSourceDataProperties lightSourceData;
        if (this.DebugCameraID > this.allLightSourceData.Length)
        {
            // Invalid camera ID
            Debug.LogWarning("Invalid camera ID");
            return;
        }

        RenderTexture rt = null;
        lightSourceData = this.allLightSourceData[this.DebugCameraID];
        if (this.TextureToRender == 0)
        {
            Debug.Log("Rendering receiver positions");
            rt = lightSourceData.ReceivingPositionTexture;
        }
        else if (this.TextureToRender == 1)
        {
            Debug.Log("Rendering splat positions");
            rt = lightSourceData.DebugSplatPosTexture;
        }
        else
        {
            Debug.LogWarning("Invalid texture to render ID");
            return;
        }

        Texture2D texture = this.ConvertRenderTextureTo2DTexture(rt);
        Color[] allColorsInTexture = texture.GetPixels();

        int count = 0;
        foreach(Color c in allColorsInTexture)
        {
            if (c.a == 0)
            {
                continue;
            }

            if (this.DebugDensity <= 1 || (count + 1) % this.DebugDensity == 0)
            {
                Vector3 worldPos = new Vector3(c.r, c.g, c.b);
                GameObject positionSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                positionSphere.transform.position = worldPos;
                positionSphere.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                positionSphere.GetComponent<Renderer>().material.SetColor("_Color", Color.green/*refractionColor*/);
                this.debugObjects.Add(positionSphere);
            }

            count++;
        }
    }

    private Texture2D ConvertRenderTextureTo2DTexture(RenderTexture rt)
    {
        RenderTexture.active = rt;
        Texture2D texture = new Texture2D(rt.width, rt.height, TextureFormat.RGBAFloat, false);
        texture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        texture.Apply();

        return texture;
    }
}