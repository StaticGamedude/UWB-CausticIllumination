using UnityEngine;
using System.Collections;

//
public partial class MyMesh : MonoBehaviour {
    private Transform mTextureTransform;
    
    void InitializeTextureTransform()
    {
        mTextureTransform = (new GameObject("Texture Transform")).transform;
    }

    void ComputeUV(Vector2[] uv)
    {
        float delta = 2f / (float)(mResolution - 1);
        for (int y = 0; y < mResolution; y++)
        {
            for (int x = 0; x < mResolution; x++)
            {
                int index = PosToVertexIndex(x, y);
                uv[index] = new Vector2(x * delta * 0.5f, y * delta * 0.5f);
            }
        }
    }

    public Transform GetTextureTransform() { return mTextureTransform; }

    // Use this for initialization
    void UpdateTextureTransform(Vector2[] uv)
    {
        Matrix3x3 t = Matrix3x3Helpers.CreateTranslation(new 
                            Vector2(mTextureTransform.localPosition.x, mTextureTransform.localPosition.y));
        Matrix3x3 s = Matrix3x3Helpers.CreateScale(new
                            Vector2(mTextureTransform.localScale.x, mTextureTransform.localScale.y));
        Matrix3x3 r = Matrix3x3Helpers.CreateRotation(mTextureTransform.localRotation.eulerAngles.z);
        Matrix3x3 m = t * r * s;

        ComputeUV(uv);
        for (int i = 0; i < uv.Length; i++)
        {
            uv[i] = Matrix3x3.MultiplyVector2(m, uv[i]);
        }
    }
}
