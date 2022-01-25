using System.Collections.Generic;
using UnityEngine;

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
    /// Determines the color of the debug objects rendered in the scene
    /// </summary>
    public Color Debug_ObjectColor = Color.green;

    /// <summary>
    /// Interal list of spheres which represent the positions read from the position texture
    /// </summary>
    private List<GameObject> debug_PositionSpheres;

    /// <summary>
    /// Defines the rendering pass that we want to investigate
    /// </summary>
    public RenderingPass PassToInspect = RenderingPass.PASS_1;

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
        RenderTexture specularPositionsTexture = null;
        RenderTexture specularNormalTexture = null;
        RenderTexture receiverPositionsTexture = null;
        List<GameObject> debugSpecularPositionObjects = null;
        List<GameObject> debugSpecularNormalObjects = null;
        List<GameObject> debugReceiverPositionObjects = null;

        switch (this.PassToInspect)
        {
            case RenderingPass.PASS_2:
                specularPositionsTexture = this.LightCameraCausticTexture;
                debugSpecularPositionObjects = this.debug_PositionSpheres;
                break;
            case RenderingPass.PASS_1:
            default:
                specularPositionsTexture = this.LightCameraRefractionPositionTexture;
                specularNormalTexture = this.LightCameraRefractionNormalTexture;
                receiverPositionsTexture = this.LightCameraReceivingPositionTexture;
                debugSpecularPositionObjects = this.debug_PositionSpheres;
                debugSpecularNormalObjects = this.debug_NormalCylinder;
                debugReceiverPositionObjects = this.debug_ReceivingPositionSpheres;
                break;
        }

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
            this.Validation_RenderTextureDetails(specularPositionsTexture, debugSpecularPositionObjects, specularNormalTexture, debugSpecularNormalObjects);
        }

        // Receiving object validation should only occur when validating the first pass
        if (this.PassToInspect == RenderingPass.PASS_1 && (this.continousValidationRendering || Input.GetKeyDown(KeyCode.R)))
        {
            this.Validation_RenderTextureDetails(receiverPositionsTexture, debugReceiverPositionObjects);
        }
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
    /// Render a sphere at the world position and a cylinder which represents the normal
    /// </summary>
    /// <param name="name">Name used for debugging to help track the objects in the scene</param>
    /// <param name="worldPosition">World position to render the sphere</param>
    /// <param name="validationCylindersList">List of game objects that the newly created position sphere should be added to</param>
    /// <param name="worldNormal">[Optional] The world normal of the vertex</param>
    /// <param name="validationCylindersList">[Optional] List of game objects that the newly created position normal cylinder should be added to</param>
    private void CreatePositionValidationObjects(string name, Vector3 worldPosition, List<GameObject> validationSpheresList, Vector3? worldNormal = null, List<GameObject> validationCylindersList = null)
    {
        GameObject positionSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        positionSphere.transform.position = worldPosition;
        positionSphere.transform.localScale = this.Debug_SpherePositionSize;
        positionSphere.name = $"Position Sphere: {name}";
        validationSpheresList?.Add(positionSphere);


        var sphereRenderer = positionSphere.GetComponent<Renderer>();
        sphereRenderer.material.SetColor("_Color", this.Debug_ObjectColor);
        if (worldNormal != null)
        {
            Vector3 normal = (Vector3)worldNormal;
            GameObject normalDirectionCylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);

            normalDirectionCylinder.transform.position = worldPosition + (normal * this.Debug_CylinderNormalSize.y);
            normalDirectionCylinder.transform.rotation = Quaternion.FromToRotation(Vector3.up, normal);
            normalDirectionCylinder.transform.localScale = this.Debug_CylinderNormalSize;
            normalDirectionCylinder.name = $"Normal cylinder: {name}";
            normalDirectionCylinder.transform.SetParent(positionSphere.transform);
            validationCylindersList?.Add(normalDirectionCylinder);

            var cylinderRenderer = normalDirectionCylinder.GetComponent<Renderer>();
            cylinderRenderer.material.SetColor("_Color", this.Debug_ObjectColor);
        }
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

    /// <summary>
    /// Given a position render texture and normal render texture, attempt to render objects to help show what was read from the
    /// textures. A sphere is rendered at each position read from the positions texture, and a cylinder is rendered to represent the 
    /// normal
    /// </summary>
    /// <param name="positionRenderTexture">Render texture containing world positions</param>
    /// <param name="validationPositionSpheres">List of game objects in which the created valitation spheres are added to</param>
    /// <param name="normalRenderTexture">[Optional] Render texture containing world normals</param>
    /// <param name="validationNormalCylinders">[Optional] List of game objects in which the created validation cylinders are added to</param>
    private void Validation_RenderTextureDetails(RenderTexture positionRenderTexture, List<GameObject> validationPositionSpheres, RenderTexture normalRenderTexture = null , List<GameObject> validationNormalCylinders = null)
    {
        Texture2D positionTexture = this.ConvertRenderTextureTo2DTexture(positionRenderTexture);
        Texture2D normalTexture = normalRenderTexture != null ? this.ConvertRenderTextureTo2DTexture(normalRenderTexture) : null;

        int count = 0;

        // Delete the previously created game objects (if any)
        validationPositionSpheres.ForEach(sphere => Destroy(sphere));
        validationNormalCylinders?.ForEach(sphere => Destroy(sphere));

        for (int row = 0; row < positionTexture.width; row++)
        {
            for (int col = 0; col < positionTexture.height; col++)
            {
                Color positionPixelColor = positionTexture.GetPixel(row, col);
                Vector3 worldPos = new Vector3(positionPixelColor.r, positionPixelColor.g, positionPixelColor.b);
                bool isVisiblePosition = positionPixelColor.a > 0; //The alpha channel of the pixel indicates whether the position is valid (i.e. seen by the light camera)
                Color? normalPixelColor = normalTexture != null ? (Color?)normalTexture.GetPixel(row, col) : null;
                Vector3? normal = normalPixelColor != null ? (Vector3?)(new Vector3(((Color)normalPixelColor).r, ((Color)normalPixelColor).g, ((Color)normalPixelColor).b)) : null;

                if (!isVisiblePosition)
                {
                    continue;
                }

                if (this.Debug_RenderEveryXElement <= 1 || (count + 1) % this.Debug_RenderEveryXElement == 0)
                {
                    this.CreatePositionValidationObjects(count.ToString(), worldPos, validationPositionSpheres, normal, validationNormalCylinders);
                }

                count++;
            }
        }
    }
}
