using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    public RenderTexture TestRenderTexture;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Assert(TestRenderTexture != null);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            Texture2D t = this.ConvertRenderTextureTo2DTexture(TestRenderTexture);
            for (int i = 0; i < t.width; i++)
            {
                for (int j = 0; j < t.height; j++)
                {
                    Color c = t.GetPixel(i, j);
                    if (c.r != 0 || c.g != 0 || c.b != 0)
                    {
                        Debug.Log($"first value found: ({c.r}, {c.g}, {c.b})");
                        break;
                    }
                }
            }
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

}
