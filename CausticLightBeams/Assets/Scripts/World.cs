using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;

public class World : MonoBehaviour
{
    private const float WAIT_TIME = 5.0f;
    private List<GameObject> testSpheres;
    private float elapsedTime = 0;
    private bool spheresCreated = false;
    private Color[] textureColorPixels;

    public Texture PositionTexture;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Assert(PositionTexture != null);

        textureColorPixels = GetAllColorValues(PositionTexture);
        testSpheres = new List<GameObject>();
    }

    private void Update()
    {
        //Wait a few seconds before attempting to render the test spheres
        elapsedTime += Time.deltaTime;
        if (elapsedTime >= WAIT_TIME)
        {
            UpdateTestSpheres();
            elapsedTime = 0;
        }
    }

    /// <summary>
    /// Because the RenderTexture isn't of the type Texture2D, we can't sample from it normally (even though
    /// the render texture is expected to 2D). As a result, we're having to try another method for extracting the color 
    /// values
    /// </summary>
    /// <param name="tex">Texture to extract colors from</param>
    /// <returns>Array of color values found in the texture</returns>
    private Color[] GetAllColorValues(Texture tex)
    {
        Texture2D texture2D = new Texture2D(tex.width, tex.height, TextureFormat.RGBA32, false);

        RenderTexture currentRT = RenderTexture.active;

        RenderTexture renderTexture = new RenderTexture(tex.width, tex.height, 32);
        Graphics.Blit(tex, renderTexture);

        RenderTexture.active = renderTexture;
        texture2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        texture2D.Apply();

        Color[] pixels = texture2D.GetPixels();

        RenderTexture.active = currentRT;
        return pixels;
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
