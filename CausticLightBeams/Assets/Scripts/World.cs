using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Timers;
using UnityEngine;

public class World : MonoBehaviour
{
    private List<GameObject> testSpheres;
    private bool spheresCreated = false;
    private Color[] textureColorPixels;

    public RenderTexture CreatedRenderTexture;
    public Camera CameraGeneratingTexture;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Assert(CreatedRenderTexture != null);
        Debug.Assert(CameraGeneratingTexture != null);

        textureColorPixels = GetAllColorValues(CreatedRenderTexture);
        testSpheres = new List<GameObject>();
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.C))
        {
            SavePosTextureToLocal();
        }
        else if (Input.GetKey(KeyCode.R))
        {
            RenderTestSpheres();
        }
    }

    /// <summary>
    /// Because the RenderTexture isn't of the type Texture2D, we can't sample from it normally (even though
    /// the render texture is expected to 2D). As a result, we're having to try another method for extracting the color 
    /// values
    /// </summary>
    /// <param name="tex">Texture to extract colors from</param>
    /// <returns>Array of color values found in the texture</returns>
    private Color[] GetAllColorValues(RenderTexture tex)
    {
        Texture2D texture2D = new Texture2D(tex.width, tex.height);

        RenderTexture currentRT = RenderTexture.active;

        RenderTexture.active = tex;
        texture2D.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
        texture2D.Apply();

        Color[] pixels = texture2D.GetPixels();

        RenderTexture.active = currentRT;

        return pixels;
    }

    private void SavePosTextureToLocal()
    {
        Debug.Log("Attempting to save image to local");

        CameraGeneratingTexture.Render();
        
        Texture2D captureTexture = TTexture2DGetRenderTexture(CreatedRenderTexture);

        byte[] data = captureTexture.EncodeToPNG();
        FileStream fs = new FileStream(@"C:/temp/test.png", FileMode.OpenOrCreate);
        fs.Write(data, 0, data.Length);
        fs.Close();


        Texture2D.DestroyImmediate(captureTexture, true);
    }


    public Texture2D TTexture2DGetRenderTexture(RenderTexture rt)
    {
        RenderTexture.active = rt;
        Texture2D tempTexture = new Texture2D(rt.width, rt.height);
        tempTexture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        tempTexture.Apply();

        return tempTexture;
    }

    private void RenderTestSpheres()
    {
        Debug.Log("Attempting to render test spheres based on position");
        foreach (GameObject testSphere in testSpheres)
        {
            Destroy(testSphere);
        }

        testSpheres = new List<GameObject>();
        int count = 0;

        CameraGeneratingTexture.Render();
        Texture2D captureTexture = TTexture2DGetRenderTexture(CreatedRenderTexture);

        for(int row = 0; row < captureTexture.width; row++)
        {
            for (int col = 0; col < captureTexture.height; col++)
            {
                Color pixelColor = captureTexture.GetPixel(row, col);
                Vector3 pos = new Vector3(pixelColor.r, pixelColor.g, pixelColor.b);

                if (pos.x != 0 || pos.y != 0 || pos.z != 0)
                {
                    GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    sphere.transform.position = pos;
                    sphere.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                    sphere.name = $"Sphere: {count}";
                    testSpheres.Add(sphere);
                    count++;
                }
            }
        }

        if (testSpheres.Count == 0)
        {
            Debug.LogWarning("No valid positions spheres found");
        }

        Texture2D.DestroyImmediate(captureTexture, true);
    }

    /// <summary>
    /// Attempt to create a sphere at every value found in the texture. Positions of (0, 0, 0) are skipped.
    /// Color values are treated as position values. Because the process of generating the spheres can take awhile
    /// we only attempt to render them once
    /// </summary>
    private void UpdateTestSpheres()
    {
        if (!spheresCreated)
        {
            Debug.Log("Updating test sphere positions");
            foreach (GameObject testSphere in testSpheres)
            {
                Destroy(testSphere);
            }

            testSpheres = new List<GameObject>();
            int count = 0;

            foreach(Color32 color in textureColorPixels)
            {
                Color32 c = textureColorPixels[count];
                Vector3 pos = new Vector3(c.r, c.g, c.b);

                if (pos.x != 0 || pos.y != 0 || pos.z != 0)
                {
                    GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    sphere.transform.position = pos;
                    sphere.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                    sphere.name = $"Sphere: {count}";
                    testSpheres.Add(sphere);
                    count++;
                }
            }

            if (testSpheres.Count == 0)
            {
                Debug.LogWarning("No valid positions spheres found");
            }

            spheresCreated = true;
        }
    }
}
