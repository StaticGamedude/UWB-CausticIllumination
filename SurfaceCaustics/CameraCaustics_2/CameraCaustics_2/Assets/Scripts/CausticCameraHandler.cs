using UnityEngine;
using System.Collections.Generic;

public class CausticCameraHandler : MonoBehaviour
{
    /// <summary>
    /// Camera responsible for capturing the caustic data. The data captured by this camera is expected to be stored in <see cref="causticCameraTargetTexture"/>
    /// </summary>
    private Camera causticCamera;

    /// <summary>
    /// Contains the output values for the caustic camera
    /// </summary>
    private RenderTexture causticCameraTargetTexture;

    /// <summary>
    /// Contains a list of objects that are used to help visualize the data in our render textures
    /// </summary>
    private List<GameObject> debugObjects = new List<GameObject>();

    /// <summary>
    /// The shader that will be set on the caustic camera as a replacement shader. <see cref="SeesSpecular"/> determines
    /// when the shader is meant to capture specular or receiving object data
    /// </summary>
    public Shader CameraReplacementShader;

    /// <summary>
    /// Determines whether the replacement shader added to the camera is set with the receiver object tag or
    /// specular object tag
    /// </summary>
    public bool SeesSpecular = true;

    /// <summary>
    /// Gets/sets the shader variable name that will contain the information of the camera's output.
    /// </summary>
    public string OutputShaderVariableName;

    /// <summary>
    /// Gets/sets the game object containing a caustic camera that is strictly used for debugging. Can be used to
    /// help render the data from <see cref="causticCameraTargetTexture"/> with the data object's caustic camera 
    /// render texture
    /// </summary>
    public GameObject SupportCausticObject;

    /// <summary>
    /// Determines whether this caustic camera is expected to capture color or float data.
    /// </summary>
    public bool CapturesColorData = false;

    /// <summary>
    /// A flag used to help render data found in the render textrure. Primarly used for debuggin
    /// </summary>
    public bool RenderPositionSpheres = false;

    /// <summary>
    /// A flag used to help indicate that we should delete our debug objects
    /// </summary>
    public bool ShouldDeleteDebugObjects = false;
    
    /// <summary>
    /// A flag used to determine whether we should render data from <see cref="SupportCausticObject"/>'s render texture
    /// </summary>
    public bool RenderSupportingDataObjects = false;

    /// <summary>
    /// Determines the amount of render objects to create when <see cref="RenderPositionSpheres"/> is enabled. We only render every
    /// <see cref="RenderingDensity"/> position when reading the data texture
    /// </summary>
    public int RenderingDensity = 50;

    public bool RenderContinuous = false;

    // Start is called before the first frame update
    void Start()
    {
        this.causticCamera = this.GetComponent<Camera>();

        Debug.Assert(this.causticCamera != null);
        Debug.Assert(this.CameraReplacementShader != null);
        Debug.Assert(!string.IsNullOrEmpty(this.OutputShaderVariableName));

        this.causticCameraTargetTexture = this.InitRenderTexture(this.CapturesColorData, $"{this.OutputShaderVariableName}RenderTexture");
        this.causticCamera.targetTexture = this.causticCameraTargetTexture;
        this.causticCamera.clearFlags = CameraClearFlags.SolidColor;
        this.causticCamera.backgroundColor = new Color(0, 0, 0, 0);
        this.causticCamera.SetReplacementShader(this.CameraReplacementShader, this.SeesSpecular ? "SpecularObject" : "ReceivingObject");
    }

    // Update is called once per frame
    void Update()
    {
        if (this.ShouldDeleteDebugObjects)
        {
            this.DeleteDebugObjects();
            this.ShouldDeleteDebugObjects = false;
        }

        if (this.RenderPositionSpheres || this.RenderContinuous)
        {
            Debug.Log("Attempting write positions");
            Texture2D positionTexture = this.ConvertRenderTextureTo2DTexture(this.causticCameraTargetTexture);
            Color[] allPixelValues = positionTexture.GetPixels();
            Color[] secondaryPixelValues = this.GetSecondaryColorvalues();
            int count = 0;

            this.DeleteDebugObjects();
            for(int i = 0; i < allPixelValues.Length; i++)
            {
                Color color = allPixelValues[i];
                if (color.a != 1)
                {
                    continue;
                }

                if (this.RenderingDensity <= 1 || (count + 1) % this.RenderingDensity == 0)
                {
                    Vector3 position = this.GetVectorFromColor(color);
                    Vector3 positionSize = new Vector3(0.3f, 0.3f, 0.3f);
                    

                    GameObject positionSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    positionSphere.transform.position = position;
                    positionSphere.transform.localScale = positionSize;
                    positionSphere.GetComponent<Renderer>().material.SetColor("_Color", Color.green);
                    this.debugObjects.Add(positionSphere);

                    if (this.RenderSupportingDataObjects && secondaryPixelValues != null)
                    {
                        Vector3 direction = this.GetVectorFromColor(secondaryPixelValues[i]);
                        Vector3 directionSize = new Vector3(0.05f, 1, 0.05f);
                        GameObject directionCylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                        directionCylinder.transform.position = position + (direction * 1);
                        directionCylinder.transform.localScale = directionSize;
                        directionCylinder.transform.rotation = Quaternion.FromToRotation(Vector3.up, direction);
                        directionCylinder.GetComponent<Renderer>().material.SetColor("_Color", Color.green);
                        this.debugObjects.Add(directionCylinder);
                    }
                }
                
                count++;
            }

            this.RenderPositionSpheres = false;
        }
    }

    private void OnPostRender()
    {
        Shader.SetGlobalTexture($"_{this.OutputShaderVariableName}", this.causticCameraTargetTexture);
    }

    public RenderTexture GetRenderTexture()
    {
        return this.causticCameraTargetTexture;
    }

    /// <summary>
    /// Create a render texture that will be used to store data from the caustic camera
    /// </summary>
    /// <param name="isColorTexture">Flag which indicates whether the texture is epxected to store color values</param>
    /// <param name="name">Name to give to the texture</param>
    /// <returns>Created render texture</returns>
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

    /// <summary>
    /// Converts a render texture to a 2D texture so that individual pixels can be read
    /// </summary>
    /// <param name="rt">Render texture to convert</param>
    /// <returns>Converted 2D texture</returns>
    private Texture2D ConvertRenderTextureTo2DTexture(RenderTexture rt)
    {
        RenderTexture.active = rt;
        Texture2D texture = new Texture2D(rt.width, rt.height, TextureFormat.RGBAFloat, false);
        texture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        texture.Apply();

        return texture;
    }

    /// <summary>
    /// Converts a color value to a Vector
    /// </summary>
    /// <param name="color">Color to convert</param>
    /// <returns>Resulting vector</returns>
    private Vector3 GetVectorFromColor(Color color)
    {
        return new Vector3(color.r, color.g, color.b);
    }

    /// <summary>
    /// Utility method to delete our debug objects
    /// </summary>
    private void DeleteDebugObjects()
    {
        this.debugObjects.ForEach(o => Destroy(o));
        this.debugObjects.Clear();
    }

    private Color[] GetSecondaryColorvalues()
    {
        if (!this.RenderSupportingDataObjects || this.SupportCausticObject == null)
        {
            return null;
        }

        CausticCameraHandler cameraHandler = this.SupportCausticObject.GetComponent<CausticCameraHandler>();
        if (cameraHandler == null)
        {
            return null;
        }

        RenderTexture supportingObjectRenderTexture = cameraHandler.GetRenderTexture();
        if (supportingObjectRenderTexture == null)
        {
            return null;
        }

        Texture2D tex = this.ConvertRenderTextureTo2DTexture(supportingObjectRenderTexture);
        return tex.GetPixels();
    }
}