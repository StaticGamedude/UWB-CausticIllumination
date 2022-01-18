using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles the overall scene logic. Reads users input and renders debug spheres and cylinders
/// at positions read from a render texture
/// </summary>
public class World : MonoBehaviour
{
    public RenderTexture[] LightCameraTargetTextures;

    /// <summary>
    /// The render texture containing the world positions of the verticies that can be
    /// seen from a light source
    /// </summary>
    public RenderTexture LightCameraPositionTexture;

    /// <summary>
    /// The render texture containing the world normals of the verticies that can be
    /// seen from a light source
    /// </summary>
    public RenderTexture LightCameraNormalTexture;

    /// <summary>
    /// Limits the number of objects created when rendering validation objects in the scene. Only every Xth item will be rendered.
    /// </summary>
    public int Debug_RenderEveryXElement = 20;

    /// <summary>
    /// Interal list of spheres which represent the positions read from the position texture
    /// </summary>
    private List<GameObject> debug_PositionSpheres;

    /// <summary>
    /// Interal list of cylinders which represents the normals ready from the normals texture
    /// </summary>
    private List<GameObject> debug_NormalCylinder;

    /// <summary>
    /// Flag indicating whether to render the validation objects in every frame
    /// </summary>
    private bool continousValidationRendering = false;

    // Start is called before the first frame update
    void Start()
    {
        debug_PositionSpheres = new List<GameObject>();
        debug_NormalCylinder = new List<GameObject>();

        Debug.Assert(LightCameraPositionTexture != null);
        Debug.Assert(LightCameraNormalTexture != null);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            this.continousValidationRendering = !this.continousValidationRendering;
        }
        else if (Input.GetKeyDown(KeyCode.C)) // Clear any validation objects in the scene
        {
            this.DeleteDebugObjects();
        }

        if (this.continousValidationRendering || Input.GetKeyDown(KeyCode.R))
        {
            this.RenderTextureDetails(this.LightCameraPositionTexture, this.LightCameraNormalTexture);
        }
    }

    /// <summary>
    /// Given a position render texture and normal render texture, attempt to render objects to help show what was read from the
    /// textures. A sphere is rendered at each position read from the positions texture, and a cylinder is rendered to represent the 
    /// normal
    /// </summary>
    /// <param name="positionRenderTexture">Render texture containing world positions</param>
    /// <param name="normalRenderTexture">Render texture containing world normals</param>
    private void RenderTextureDetails(RenderTexture positionRenderTexture, RenderTexture normalRenderTexture)
    {
        Texture2D positionTexture = this.ConvertRenderTextureTo2DTexture(positionRenderTexture);
        Texture2D normalTexture = this.ConvertRenderTextureTo2DTexture(normalRenderTexture);

        int count = 0;

        this.DeleteDebugObjects();

        for (int row = 0; row < positionTexture.width; row++)
        {
            for (int col = 0; col < positionTexture.height; col++)
            {
                Color positionPixelColor = positionTexture.GetPixel(row, col);
                Color normalPixelColor = normalTexture.GetPixel(row, col);
                Vector3 worldPos = new Vector3(positionPixelColor.r, positionPixelColor.g, positionPixelColor.b);
                Vector3 normal = new Vector3(normalPixelColor.r, normalPixelColor.g, normalPixelColor.b);
                bool isVisiblePosition = positionPixelColor.a > 0; //The alpha channel of the world position indicates whether the position is valid (i.e. seen by the light camera)

                if (!isVisiblePosition)
                {
                    continue;
                }

                if (this.Debug_RenderEveryXElement <= 1 || (count + 1) % this.Debug_RenderEveryXElement == 0)
                {
                    this.CreatePositionDebugObjects(worldPos, normal, count.ToString());
                }

                count++;
            }
        }
    }

    /// <summary>
    /// Delete the debug spheres and cylinders in the scene
    /// </summary>
    private void DeleteDebugObjects()
    {
        this.debug_PositionSpheres.ForEach(sphere => Destroy(sphere));
        this.debug_NormalCylinder.ForEach(sphere => Destroy(sphere));
    }

    /// <summary>
    /// Render a sphere at the world position and a cylinder which represents the normal
    /// </summary>
    /// <param name="worldPosition">World position to render the sphere</param>
    /// <param name="worldNormal">The world normal of the vertex</param>
    /// <param name="name">Name used for debugging to help track the objects in the scene</param>
    private void CreatePositionDebugObjects(Vector3 worldPosition, Vector3 worldNormal, string name)
    {
        GameObject positionSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        GameObject normalDirectionCylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);

        positionSphere.transform.position = worldPosition;
        normalDirectionCylinder.transform.position = worldPosition + (worldNormal * 0.02f);

        normalDirectionCylinder.transform.rotation = Quaternion.FromToRotation(Vector3.up, worldNormal);

        positionSphere.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
        normalDirectionCylinder.transform.localScale = new Vector3(0.003f, 0.02f, 0.0003f);

        positionSphere.name = $"Position Sphere: {name}";
        normalDirectionCylinder.name = $"Normal cylinder: {name}";

        normalDirectionCylinder.transform.SetParent(positionSphere.transform);
        debug_PositionSpheres.Add(positionSphere);
        debug_NormalCylinder.Add(normalDirectionCylinder);
    }

    /// <summary>
    /// Convert a render texture to a 2D texture
    /// </summary>
    /// <param name="rt"></param>
    /// <returns></returns>
    private Texture2D ConvertRenderTextureTo2DTexture(RenderTexture rt)
    {
        RenderTexture.active = rt;
        Texture2D texture = new Texture2D(rt.width, rt.height, TextureFormat.RGBAFloat, false);
        texture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        texture.Apply();

        return texture;
    }
}
