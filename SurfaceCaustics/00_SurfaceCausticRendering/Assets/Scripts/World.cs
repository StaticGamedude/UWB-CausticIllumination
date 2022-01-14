using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    public RenderTexture[] LightCameraTargetTextures;

    public RenderTexture LightCameraPositionTexture;

    public RenderTexture LightCameraNormalTexture;

    public Material LightSourceMaterial;

    public bool Debug_IgnorePositionsAtOrigin = true;

    public int Debug_NumberOfPositionsToRender = 1000;

    private List<GameObject> debug_PositionSpheres;

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
        //var renderTextures = new RenderTexture[] { this.LightCameraRenderTexture };
        
        this.MultiTargetBlit(this.LightCameraTargetTextures, this.LightSourceMaterial);
        if (Input.GetKey(KeyCode.R))
        {
            RenderTextureDetails(this.LightCameraPositionTexture, this.LightCameraNormalTexture);
            //RenderTestSpheres(renderTextures[0]);
            //RenderTestSpheres(this.LightCameraRenderTexture);
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
                
                //if (this.Debug_IgnorePositionsAtOrigin && (worldPos.Equals(Vector3.zero) || normal.Equals(Vector3.zero)))
                //{

                //}

                if (worldPos.x != 0 && worldPos.y != 0 && worldPos.z != 0 /*&& count < Debug_NumberOfPositionsToRender*/)
                {
                    this.CreatePositionDebugObjects(worldPos, normal, count.ToString());
                    count++;
                }
            }
        }
    }

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

    private Texture2D ConvertRenderTextureTo2DTexture(RenderTexture rt)
    {
        RenderTexture.active = rt;
        Texture2D texture = new Texture2D(rt.width, rt.height, TextureFormat.RGBAFloat, false);
        texture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        texture.Apply();

        return texture;
    }
}
