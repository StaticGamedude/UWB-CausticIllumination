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
    /// The render texture containing the final intensity/caustic information
    /// </summary>
    public RenderTexture LightCameraCausticFinalTexture;

    public RenderTexture LightCameraRefractionRayTexture;

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
        Shader.SetGlobalTexture("_CausticTexture", this.LightCameraCausticFinalTexture);

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
        if (Input.GetKeyDown(KeyCode.D))
        {
            this.DeleteValidationObjects();
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            this.continousValidationRendering = !this.continousValidationRendering;
        }

        if (this.continousValidationRendering || Input.GetKeyDown(KeyCode.S))
        {
            this.RenderRefractionDetails(this.LightCameraRefractionPositionTexture, this.LightCameraCausticTexture, this.LightCameraRefractionRayTexture, this.LightCameraCausticColorTexture);
        }

        this.RenderStatusText.text = $"Continous rendering " + (this.continousValidationRendering ? "enabled" : "disabled");
    }

    private void RenderRefractionDetails(RenderTexture specularPositionTexture, RenderTexture specularCausticPositionTexture, RenderTexture specularRefractionRayTexture, RenderTexture specularColorTexture)
    {
        Texture2D positionTexture = this.ConvertRenderTextureTo2DTexture(specularPositionTexture);
        Texture2D causticTexture = this.ConvertRenderTextureTo2DTexture(specularCausticPositionTexture);
        Texture2D refractionRayTexture = this.ConvertRenderTextureTo2DTexture(specularRefractionRayTexture);
        Texture2D colorTexture = this.ConvertRenderTextureTo2DTexture(specularColorTexture);
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
                    float distanceFromWorldToSplat = worldToSplat.magnitude;

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

    private Vector3 GetVectorFromColor(Color color)
    {
        return new Vector3(color.r, color.g, color.b);
    }
}
