using System.Collections;
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
    /// Material which uses the light camera position shader position
    /// </summary>
    public Material LightSourceMaterial;

    /// <summary>
    /// Debug option - do not render points if the position read from the position render texture
    /// is at the origin
    /// </summary>
    public bool Debug_IgnorePositionsAtOrigin = true;

    /// <summary>
    /// Debug option - flag which determines whether to render all possible spheres when reading from
    /// the positions texture
    /// </summary>
    public bool Debug_LimitNumberOfSpheresRendered = false;

    /// <summary>
    /// Debug option - Render a set number of position nodes and normals
    /// </summary>
    public int Debug_NumberOfPositionsToRender = 1000;

    /// <summary>
    /// Interal list of spheres which represent the positions read from the position texture
    /// </summary>
    private List<GameObject> debug_PositionSpheres;

    /// <summary>
    /// Interal list of cylinders which represents the normals ready from the normals texture
    /// </summary>
    private List<GameObject> debug_NormalCylinder;

    // Start is called before the first frame update
    void Start()
    {
        debug_PositionSpheres = new List<GameObject>();
        debug_NormalCylinder = new List<GameObject>();

        Debug.Assert(LightCameraPositionTexture != null);
        Debug.Assert(LightCameraNormalTexture != null);
        Debug.Assert(LightSourceMaterial != null);
    }

    // Update is called once per frame
    void Update()
    {
        this.MultiTargetBlit(this.LightCameraTargetTextures, this.LightSourceMaterial);
        if (Input.GetKey(KeyCode.R))
        {
            RenderTextureDetails(this.LightCameraPositionTexture, this.LightCameraNormalTexture);
        }
    }

    /// <summary>
    /// Renders into a array of render textures using multi-target blit.
    /// Up to 4 render targets are supported in Unity but some GPU's can
    /// support up to 8 so this may change in the future. You MUST set up
    /// the materials shader correctly for multitarget blit for this to work.
    /// </summary>
    /// <param name="des">The destination render textures.</param>
    /// <param name="mat">The material to use</param>
    /// <param name="pass">Which pass of the materials shader to use.</param>
    private void MultiTargetBlit(RenderTexture[] des, Material mat, int pass = 0)
    {
        RenderBuffer[] rb = new RenderBuffer[des.Length];

        // Set targets needs the color buffers so make a array from
        // each textures buffer.
        for (int i = 0; i < des.Length; i++)
            rb[i] = des[i].colorBuffer;

        //Set the targets to render into.
        //Will use the depth buffer of the
        //first render texture provided.
        Graphics.SetRenderTarget(rb, des[0].depthBuffer);

        GL.Clear(true, true, Color.clear);

        GL.PushMatrix(); // Saves the current matrix stack
        GL.LoadOrtho(); // Saves the current matrix stack

        mat.SetPass(pass); 

        GL.Begin(GL.QUADS);
        GL.TexCoord2(0.0f, 0.0f); GL.Vertex3(0.0f, 0.0f, 0.1f);
        GL.TexCoord2(1.0f, 0.0f); GL.Vertex3(1.0f, 0.0f, 0.1f);
        GL.TexCoord2(1.0f, 1.0f); GL.Vertex3(1.0f, 1.0f, 0.1f);
        GL.TexCoord2(0.0f, 1.0f); GL.Vertex3(0.0f, 1.0f, 0.1f);
        GL.End();

        GL.PopMatrix(); // restores the current matrix stack
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

        debug_PositionSpheres.ForEach(sphere => Destroy(sphere));
        debug_NormalCylinder.ForEach(sphere => Destroy(sphere));

        for (int row = 0; row < positionTexture.width; row++)
        {
            for (int col = 0; col < positionTexture.height; col++)
            {
                Color positionPixelColor = positionTexture.GetPixel(row, col);
                Color normalPixelColor = normalTexture.GetPixel(row, col);
                Vector3 worldPos = new Vector3(positionPixelColor.r, positionPixelColor.g, positionPixelColor.b);
                Vector3 normal = new Vector3(normalPixelColor.r, normalPixelColor.g, normalPixelColor.b);

                if (this.Debug_IgnorePositionsAtOrigin && (worldPos.Equals(Vector3.zero) || normal.Equals(Vector3.zero)))
                {
                    continue;
                }

                if (this.Debug_LimitNumberOfSpheresRendered && count < Debug_NumberOfPositionsToRender)
                {
                    break;
                }

                this.CreatePositionDebugObjects(worldPos, normal, count.ToString());
                count++;
            }
        }
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
