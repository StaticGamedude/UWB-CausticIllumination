using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    public RenderTexture LightCausticDistanceTexture;

    /// <summary>
    /// The render texture containing the final intensity/caustic information
    /// </summary>
    public RenderTexture LightCameraCausticFinalTexture;

    public RenderTexture LightCameraRefractionRayTexture;

    public GameObject DiffuseObject;

    public Text RenderStatusText;

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

    public float Debug_LightIntensity = 1;

    public float Debug_AbsorbtionCoefficient = 0.00017f;

    public bool Debug_RenderSpecularPositions = true;

    public bool Debug_RenderRefractionDirection = true;

    public bool Debug_RenderCausticPositions = true;

    public float Debug_IlluminationDistance = 1;

    public float Debug_Flux = 0.5f;

    public float Debug_Flux_Multiplier = 500;

    public int Debug_NumOfVisiblePixels = 600;

    public float SeparatingDistance = 0.5f;

    public bool Debug_TransformSpecularGeometry = true;

    public int Debug_QuickDistanceTest = 1;

    public bool Debug_AllowNegativeIntensities = true;

    public bool Debug_MultiplyIntensity = false;

    public Color Debug_LightColor = Color.white;

    public DebugEstimationStep Debug_EstimationStep = DebugEstimationStep.INVERSE_REFRACTION_DIRECTION;

    /// <summary>
    /// Interal list of spheres which represent the positions read from the position texture
    /// </summary>
    private List<GameObject> debug_PositionSpheres;

    /// <summary>
    /// Interal list of cylinders which represents the normals ready from the normals texture
    /// </summary>
    private List<GameObject> debug_NormalCylinder;

    /// <summary>
    /// Interal list of spheres which represent the positions read from the receiving object's position texture
    /// </summary>
    private List<GameObject> debug_ReceivingPositionSpheres;

    /// <summary>
    /// Flag indicating whether to render the validation objects in every frame
    /// </summary>
    private bool continousValidationRendering = false;

    // Start is called before the first frame update
    void Start()
    {
        debug_PositionSpheres = new List<GameObject>();
        debug_NormalCylinder = new List<GameObject>();
        debug_ReceivingPositionSpheres = new List<GameObject>();

        Debug.Assert(LightCameraRefractionPositionTexture != null);
        Debug.Assert(LightCameraRefractionNormalTexture != null);
    }

    // Update is called once per frame
    void Update()
    {
        if (this.Debug_RefractionIndex < 1)
        {
            this.Debug_RefractionIndex = 1;
        }

        Shader.SetGlobalFloat("_RefractiveIndex", this.Debug_RefractionIndex);
        Shader.SetGlobalFloat("_LightIntensity", this.Debug_LightIntensity);
        Shader.SetGlobalFloat("_IlluminationDistance", this.Debug_IlluminationDistance);
        Shader.SetGlobalVector("_DiffuseObjectPos", this.DiffuseObject.transform.position);
        Shader.SetGlobalTexture("_CausticTexture", this.LightCameraCausticFinalTexture);
        Shader.SetGlobalTexture("_CausticFluxTexture", this.LightCameraFluxTexture);
        Shader.SetGlobalFloat("_GlobalAbsorbtionCoefficient", this.Debug_AbsorbtionCoefficient);
        Shader.SetGlobalInt("_NumProjectedVerticies", /*this.GetNumberOfVisiblePixels(this.LightCameraRefractionPositionTexture)*/ this.Debug_NumOfVisiblePixels);
        Shader.SetGlobalFloat("_DebugFlux", this.Debug_Flux);
        Shader.SetGlobalFloat("_DebugFluxMultiplier", this.Debug_Flux_Multiplier);
        Shader.SetGlobalInt("_Debug_TransformSpecularGeometry", this.Debug_TransformSpecularGeometry ? 1 : 0);
        Shader.SetGlobalInt("_Debug_EstimationStep", (int)this.Debug_EstimationStep);
        Shader.SetGlobalInt("_Debug_AllowNegativeIntensities", this.Debug_AllowNegativeIntensities ? 1 : 0);
        Shader.SetGlobalInt("_Debug_MultiplyIntensity", this.Debug_MultiplyIntensity ? 1 : 0);
        Shader.SetGlobalColor("_DebugLightColor", this.Debug_LightColor);

        this.HandleValidationInputs();
    }

    /// <summary>
    /// Delete the validation spheres and cylinders in the scene
    /// </summary>
    private void DeleteValidationObjects()
    {
        this.debug_PositionSpheres.ForEach(gameObject => Destroy(gameObject));
        this.debug_NormalCylinder.ForEach(gameObject => Destroy(gameObject));
        this.debug_ReceivingPositionSpheres.ForEach(gameObject => Destroy(gameObject));
    }

  
    /// <summary>
    /// Convert a render texture to a 2D texture
    /// </summary>
    /// <param name="rt">Render texture to convert</param>
    /// <returns>The convereted texture</returns>
    private Texture2D ConvertRenderTextureTo2DTexture(RenderTexture rt)
    {
        RenderTexture.active = rt;
        Texture2D texture = new Texture2D(rt.width, rt.height, TextureFormat.RGBAFloat, false);
        texture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        texture.Apply();

        return texture;
    }

    private void HandleValidationInputs()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            this.PrintDiscoveredFluxValue();
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            this.DeleteValidationObjects();
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            this.continousValidationRendering = !this.continousValidationRendering;
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            this.RenderRefractionDetails(this.LightCameraRefractionPositionTexture, this.LightCameraCausticTexture, this.LightCameraRefractionRayTexture, this.LightCameraCausticColorTexture);
        }
        else if (this.continousValidationRendering || Input.GetKeyDown(KeyCode.S))
        {
            this.RenderRefractionDetails_2(this.LightCameraRefractionPositionTexture, this.LightCameraCausticTexture, this.LightCameraRefractionRayTexture, this.LightCameraCausticColorTexture);
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            this.Test();
        }

        this.RenderStatusText.text = $"Continous rendering " + (this.continousValidationRendering ? "enabled" : "disabled");
    }

    private void RenderRefractionDetails(RenderTexture specularPositionTexture, RenderTexture specularCausticPositionTexture, RenderTexture specularRefractionRayTexture, RenderTexture specularColorTexture)
    {
        Texture2D positionTexture = this.ConvertRenderTextureTo2DTexture(specularPositionTexture);
        Texture2D causticTexture = this.ConvertRenderTextureTo2DTexture(specularCausticPositionTexture);
        Texture2D refractionRayTexture = this.ConvertRenderTextureTo2DTexture(specularRefractionRayTexture);
        Texture2D colorTexture = this.ConvertRenderTextureTo2DTexture(specularColorTexture);
        Texture2D distanceTexture = this.ConvertRenderTextureTo2DTexture(this.LightCausticDistanceTexture);
        Texture2D fluxTexture = this.ConvertRenderTextureTo2DTexture(this.LightCameraFluxTexture);

        int count = 0;

        this.debug_PositionSpheres.ForEach(sphere => Destroy(sphere));
        this.debug_PositionSpheres.Clear();
        this.debug_NormalCylinder?.ForEach(sphere => Destroy(sphere));
        this.debug_NormalCylinder?.Clear();

        for (int row = 0; row < positionTexture.width; row++)
        {
            for (int col = 0; col < positionTexture.height; col++)
            {
                bool isVisiblePosition = positionTexture.GetPixel(row, col).a > 0; //The alpha channel of the pixel indicates whether the position is valid (i.e. seen by the light camera)
                if (!isVisiblePosition)
                {
                    continue;
                }

                if (this.Debug_RenderEveryXElement <= 1 || (count + 1) % this.Debug_RenderEveryXElement == 0)
                {
                    Vector3 worldPos = this.GetVectorFromColor(positionTexture.GetPixel(row, col));
                    Vector3 splatPos = this.GetVectorFromColor(causticTexture.GetPixel(row, col));
                    Vector3 refractionRay = this.GetVectorFromColor(refractionRayTexture.GetPixel(row, col));
                    Color refractionColor = colorTexture.GetPixel(row, col);
                    Vector3 worldToSplat = splatPos - worldPos;
                    //float distanceFromWorldToSplat = worldToSplat.magnitude;
                    float distanceFromWorldToSplat = this.GetVectorFromColor(distanceTexture.GetPixel(row, col)).x;

                    if (this.Debug_RenderSpecularPositions)
                    {
                        GameObject positionSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        positionSphere.transform.position = worldPos;
                        positionSphere.transform.localScale = this.Debug_SpherePositionSize;
                        positionSphere.name = $"Position Sphere: {name}";
                        positionSphere.GetComponent<Renderer>().material.SetColor("_Color", refractionColor);
                        this.debug_PositionSpheres.Add(positionSphere);
                    }
                    
                    if (this.Debug_RenderCausticPositions)
                    {
                        GameObject splatPositionSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        splatPositionSphere.transform.position = splatPos;
                        splatPositionSphere.transform.localScale = this.Debug_SpherePositionSize;
                        splatPositionSphere.name = $"Splat Position Sphere: {name}";
                        splatPositionSphere.GetComponent<Renderer>().material.SetColor("_Color", refractionColor);
                        this.debug_PositionSpheres.Add(splatPositionSphere);
                    }
                    
                    if (this.Debug_RenderRefractionDirection)
                    {
                        GameObject normalDirectionCylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                        normalDirectionCylinder.transform.position = worldPos + (distanceFromWorldToSplat / 2 * refractionRay.normalized);
                        normalDirectionCylinder.transform.rotation = Quaternion.FromToRotation(Vector3.up, refractionRay);
                        normalDirectionCylinder.transform.localScale = new Vector3(0.01f, distanceFromWorldToSplat / 2, 0.01f);
                        normalDirectionCylinder.name = $"Refraction ray cylinder: {name}";
                        normalDirectionCylinder.GetComponent<Renderer>().material.SetColor("_Color", refractionColor);
                        this.debug_NormalCylinder.Add(normalDirectionCylinder);
                    }
                }

                count++;
            }
        }
    }

    private void RenderRefractionDetails_2(RenderTexture specularPositionTexture, RenderTexture specularCausticPositionTexture, RenderTexture specularRefractionRayTexture, RenderTexture specularColorTexture)
    {
        Texture2D positionTexture = this.ConvertRenderTextureTo2DTexture(specularPositionTexture);
        Texture2D causticTexture = this.ConvertRenderTextureTo2DTexture(specularCausticPositionTexture);
        Texture2D refractionRayTexture = this.ConvertRenderTextureTo2DTexture(specularRefractionRayTexture);
        Texture2D colorTexture = this.ConvertRenderTextureTo2DTexture(specularColorTexture);
        Texture2D distanceTexture = this.ConvertRenderTextureTo2DTexture(this.LightCausticDistanceTexture);
        Texture2D fluxTexture = this.ConvertRenderTextureTo2DTexture(this.LightCameraFluxTexture);

        int count = 0;

        this.debug_PositionSpheres.ForEach(sphere => Destroy(sphere));
        this.debug_PositionSpheres.Clear();
        this.debug_NormalCylinder?.ForEach(sphere => Destroy(sphere));
        this.debug_NormalCylinder?.Clear();

        for (int row = 0; row < positionTexture.width; row++)
        {
            for (int col = 0; col < positionTexture.height; col++)
            {
                bool specularPosVisible = this.IsVisiblePosition(positionTexture, row, col);
                bool splatPosVisible = this.IsVisiblePosition(causticTexture, row, col);

                if (this.Debug_RenderEveryXElement <= 1 || (count + 1) % this.Debug_RenderEveryXElement == 0)
                {
                    if (specularPosVisible && this.Debug_RenderSpecularPositions)
                    {
                        Vector3 worldPos = this.GetVectorFromColor(positionTexture.GetPixel(row, col));
                        GameObject positionSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        positionSphere.transform.position = worldPos;
                        positionSphere.transform.localScale = this.Debug_SpherePositionSize;
                        positionSphere.name = $"Position Sphere: {name}";
                        positionSphere.GetComponent<Renderer>().material.SetColor("_Color", Color.green/*refractionColor*/);
                        this.debug_PositionSpheres.Add(positionSphere);
                    }

                    if (splatPosVisible && this.Debug_RenderCausticPositions)
                    {
                        Vector3 splatPos = this.GetVectorFromColor(causticTexture.GetPixel(row, col));
                        GameObject splatPositionSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        splatPositionSphere.transform.position = splatPos;
                        splatPositionSphere.transform.localScale = this.Debug_SpherePositionSize;
                        splatPositionSphere.name = $"Splat Position Sphere: {name}";
                        splatPositionSphere.GetComponent<Renderer>().material.SetColor("_Color", Color.blue/*refractionColor*/);
                        this.debug_PositionSpheres.Add(splatPositionSphere);
                    }
                }

                count++;
            }
        }
    }

    private Vector3 GetVectorFromColor(Color color)
    {
        return new Vector3(color.r, color.g, color.b);
    }

    private int GetNumberOfVisiblePixels(RenderTexture specularPositionTexture)
    {
        Texture2D positionTexture = this.ConvertRenderTextureTo2DTexture(specularPositionTexture);
        int count = 0;
        for (int row = 0; row < positionTexture.width; row++)
        {
            for (int col = 0; col < positionTexture.height; col++)
            {
                bool isVisiblePosition = positionTexture.GetPixel(row, col).a > 0; //The alpha channel of the pixel indicates whether the position is valid (i.e. seen by the light camera)
                if (isVisiblePosition)
                {
                    count++;
                }
            }
        }

        return count;
    }

    private bool IsVisiblePosition(Texture2D texture, int row, int col)
    {
        return texture.GetPixel(row, col).a > 0;
    }

    private void PrintDiscoveredFluxValue()
    {
        Texture2D fluxTexture = this.ConvertRenderTextureTo2DTexture(this.LightCameraFluxTexture);
        for (int row = 0; row < fluxTexture.width; row++)
        {
            for (int col = 0; col < fluxTexture.height; col++)
            {
                Color color = fluxTexture.GetPixel(row, col);
                if (color.a > 0)
                {
                    Debug.Log($"Found color value: {color.r}, {color.g}, {color.b}");
                    return;
                }
            }
        }
    }

    private void Test()
    {
        //Debug.Log("Checking for nearby points");
        //Texture2D causticTexture = this.ConvertRenderTextureTo2DTexture(this.LightCameraCausticTexture);

        //List<Vector3> allPositions = new List<Vector3>();
        //for(int i = 0; i < causticTexture.width; i++)
        //{
        //    for (int j = 0; j < causticTexture.height; j++)
        //    {
        //        Color color = causticTexture.GetPixel(i, j);
        //        if (color.a > 0)
        //        {
        //            allPositions.Add(this.GetVectorFromColor(causticTexture.GetPixel(i, j)));
        //        }
        //    }
        //}

        //foreach(Vector3 splatPos in allPositions)
        //{
        //    var nearbyPositions = allPositions.Where(p => p != splatPos && Vector3.Distance(p, splatPos) < /*this.SeparatingDistance*/float.Epsilon).ToList();
        //    if (nearbyPositions.Count > 0)
        //    {
        //        Debug.Log($"({splatPos.x}, {splatPos.y}, {splatPos.z} has {nearbyPositions.Count} nearby positions");
        //    }
        //}

        //Debug.Log("Nearby check complete");


        Debug.Log("Checking for negative values in flux texture");
        Texture2D causticTexture = this.ConvertRenderTextureTo2DTexture(this.LightCameraFluxTexture);

        List<Vector3> allPositions = new List<Vector3>();
        for (int i = 0; i < causticTexture.width; i++)
        {
            for (int j = 0; j < causticTexture.height; j++)
            {
                Color color = causticTexture.GetPixel(i, j);
                if (color.r < 0)
                {
                    Debug.Log($"found negative value: {color}");
                }
            }
        }

        foreach (Vector3 splatPos in allPositions)
        {
            var nearbyPositions = allPositions.Where(p => p != splatPos && Vector3.Distance(p, splatPos) < /*this.SeparatingDistance*/float.Epsilon).ToList();
            if (nearbyPositions.Count > 0)
            {
                Debug.Log($"({splatPos.x}, {splatPos.y}, {splatPos.z} has {nearbyPositions.Count} nearby positions");
            }
        }

        Debug.Log("Negative check complete");
    }
}
