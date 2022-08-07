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

    private bool analyzingFPS = false;

    private float fpsTotal = 0;

    private int frameCount = 0;

    public int NumOfFramesToTest = 60;

    /// <summary>
    /// Gets/sets the value multiplied the flux values read in the flux texture
    /// </summary>
    public float Flux_Multiplier = 500;

    /// <summary>
    /// A flag which determines whether the caustic effect is rendered
    /// </summary>
    public bool RenderCaustics = true;

    /// <summary>
    /// Gets/sets the caustic threshold. If a value in the final caustic texture is greater than this value,
    /// we treat the position as being in the caustic-receiving position 
    /// </summary>
    public float CausticThreshold = 0.1f;

    /// <summary>
    /// Gets/sets the visible surface area of a specular object seen by a caustic camera
    /// </summary>
    public int NumOfVisiblePixels = 600;

    /// <summary>
    /// A flag which determines whether shadows are rendered
    /// </summary>
    public bool RenderShadows = true;

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

    public float DistanceTest = 1.0f;

    public float DebugFlux = 1.0f;

    public float AngleLimit = 0;

    // Start is called before the first frame update
    void Start()
    {
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

        if (this.CausticBlurKernalSize < 1)
        {
            this.CausticBlurKernalSize = 1;
        }

        Shader.SetGlobalInt("_NumProjectedVerticies", this.NumOfVisiblePixels);
        Shader.SetGlobalFloat("_DebugFluxMultiplier", this.Flux_Multiplier);
        Shader.SetGlobalInt("_RenderShadows", this.RenderShadows ? 1 : 0);
        Shader.SetGlobalInt("_RenderCaustics", this.RenderCaustics ? 1 : 0);
        Shader.SetGlobalFloat("_ShadowThreshold", this.ShadowThreshold);
        Shader.SetGlobalFloat("_CausticThreshold", this.CausticThreshold);
        Shader.SetGlobalInt("_CausticBlurKernalSize", this.CausticBlurKernalSize);
        Shader.SetGlobalInt("_ShadowBlurKernelSize", this.ShadowBlurKernelSize);
        Shader.SetGlobalFloat("_DrewDistanceTest", this.DistanceTest);
        Shader.SetGlobalFloat("_DebugFlux", this.DebugFlux);
        Shader.SetGlobalFloat("_AngleLimit", this.AngleLimit);

        Shader.SetGlobalFloatArray("_AllLightIds", lightIDs);

        if (!this.analyzingFPS && Input.GetKeyDown(KeyCode.A))
        {
            this.analyzingFPS = true;
            Debug.Log("Analyzing FPS");
        }

        if (this.analyzingFPS && frameCount < this.NumOfFramesToTest)
        {
            this.fpsTotal += (1 / Time.deltaTime);
            this.frameCount++;
        }
        else if (this.analyzingFPS)
        {
            this.analyzingFPS = false;
            float averageFPS = this.fpsTotal / this.NumOfFramesToTest;
            Debug.Log($"Average FPS: {averageFPS}");
            this.fpsTotal = 0;
            this.frameCount = 0;
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