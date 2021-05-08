using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    public RenderTexture[] TestTextures;
    public Material TestMaterial;

    public Material DepthMaterial;

    public GameObject DepthCube;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Assert(TestTextures != null);
        Debug.Assert(TestTextures.Length > 0);
        Debug.Assert(TestMaterial != null);
        Debug.Assert(DepthMaterial != null);
    }

    private void OnPostRender()
    {
        //this.MultiTargetBlit(this.TestTextures, this.TestMaterial);
    }

    private void Update()
    {
        this.MultiTargetBlit(this.TestTextures, this.TestMaterial);

        if (Input.GetKey(KeyCode.Alpha1))
        {
            PrintFirstTextureValues(0);
        }
        else if (Input.GetKey(KeyCode.Alpha2))
        {
            PrintFirstTextureValues(1);
        }
        else if (Input.GetKey(KeyCode.Alpha3))
        {
            PrintFirstTextureValues(2);
        }
        else if (Input.GetKey(KeyCode.Alpha4))
        {
            PrintFirstTextureValues(3);
        }

        if (Input.GetKey(KeyCode.D))
        {
            this.PrintTestDepthTextureValues();
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
            //des[i].
            rb[i] = des[i].colorBuffer;

        //Set the targets to render into.
        //Will use the depth buffer of the
        //first render texture provided.
        Graphics.SetRenderTarget(rb, des[0].depthBuffer);

        GL.Clear(true, true, Color.clear);

        GL.PushMatrix();
        GL.LoadOrtho();

        mat.SetPass(pass);

        GL.Begin(GL.QUADS);
        GL.TexCoord2(0.0f, 0.0f); GL.Vertex3(0.0f, 0.0f, 0.1f);
        GL.TexCoord2(1.0f, 0.0f); GL.Vertex3(1.0f, 0.0f, 0.1f);
        GL.TexCoord2(1.0f, 1.0f); GL.Vertex3(1.0f, 1.0f, 0.1f);
        GL.TexCoord2(0.0f, 1.0f); GL.Vertex3(0.0f, 1.0f, 0.1f);
        GL.End();

        GL.PopMatrix();
    }

    private void DepthTargetBlit(RenderTexture des, Material mat)
    {
        RenderBuffer rb = des.colorBuffer;

        //Set the targets to render into.
        //Will use the depth buffer of the
        //first render texture provided.
        Graphics.SetRenderTarget(rb, des.depthBuffer);

        GL.Clear(true, true, Color.clear);

        GL.PushMatrix();
        GL.LoadOrtho();

        mat.SetPass(0);

        GL.Begin(GL.QUADS);
        GL.TexCoord2(0.0f, 0.0f); GL.Vertex3(0.0f, 0.0f, 0.1f);
        GL.TexCoord2(1.0f, 0.0f); GL.Vertex3(1.0f, 0.0f, 0.1f);
        GL.TexCoord2(1.0f, 1.0f); GL.Vertex3(1.0f, 1.0f, 0.1f);
        GL.TexCoord2(0.0f, 1.0f); GL.Vertex3(0.0f, 1.0f, 0.1f);
        GL.End();

        GL.PopMatrix();
    }

    public Texture2D TTexture2DGetRenderTexture(RenderTexture rt)
    {
        RenderTexture.active = rt;
        Texture2D tempTexture = new Texture2D(rt.width, rt.height);
        tempTexture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        tempTexture.Apply();

        return tempTexture;
    }

    private void PrintFirstTextureValues(int textureIndex)
    {
        Debug.Log($"Print texture color values for texture: {textureIndex}");
        

        Texture2D captureTexture = TTexture2DGetRenderTexture(TestTextures[textureIndex]);
        var rawTextureData = captureTexture.GetRawTextureData<Color32>();

        foreach (Color32 c in rawTextureData)
        {
            Debug.Log($"R: {c.r}, G:{c.g}, B:{c.b}, A:{c.a}");
        }

        Texture2D.DestroyImmediate(captureTexture, true);
    }

    private Texture2D TextureToTexture2D(Texture texture)
    {
        if (texture != null)
        {
            Texture2D texture2D = new Texture2D(texture.width, texture.height, TextureFormat.RFloat, false);

            RenderTexture currentRT = RenderTexture.active;

            RenderTexture renderTexture = new RenderTexture(texture.width, texture.height, 32);
            Graphics.Blit(texture, renderTexture);

            RenderTexture.active = renderTexture;
            texture2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            texture2D.Apply();
            return texture2D;
        }
        return null;
    }

    private void PrintTestDepthTextureValues()
    {
        Debug.Log($"Print texture depth values");

        Texture2D captureTexture = TextureToTexture2D(DepthMaterial.mainTexture);
        var rawTextureData = captureTexture.GetRawTextureData<float>();

        foreach (float f in rawTextureData)
        {
            Debug.Log($"value: {f}");
        }

        Texture2D.DestroyImmediate(captureTexture, true);
    }
}
