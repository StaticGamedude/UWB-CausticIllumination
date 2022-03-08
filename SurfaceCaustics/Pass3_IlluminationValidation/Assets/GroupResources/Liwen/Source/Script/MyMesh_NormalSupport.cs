using UnityEngine;
using System.Collections;

// Read about mesh: https://docs.unity3d.com/ScriptReference/Mesh.html
//
public partial class MyMesh : MonoBehaviour {

    TriangleIndices ti = new TriangleIndices(); // this is to conserve number of times we need to new

    // (x,y): column/row lower-left position, ul: either upper(1) or lower(0)
    //      computes the normal vector for this triangle
    Vector3 TriangleNormal(Vector3[] v, int x, int y, int ul)
    {
        GetTriangleIndices(x, y, ref ti);
        Vector3 v1 = v[ti.v[ul,2]] - v[ti.v[ul,0]];
        Vector3 v2 = v[ti.v[ul,1]] - v[ti.v[ul,0]];
        return Vector3.Cross(v2, v1);
    }
    /*
    
    private Vector4 ComputeTangent(Vector3 normal)
    {
        Vector4 tangent;
        Vector3 t1 = Vector3.Cross(normal, Vector3.forward);
        Vector3 t2 = Vector3.Cross(normal, Vector3.up);
        if (t1.magnitude > t2.magnitude)
        {
            tangent = t1;
        }
        else
        {
            tangent = t2;
        }
        return tangent;
    }

    void ComputeTangent(Vector3[] n, Vector4[] tan)
    {
        for (int i = 0; i<n.Length; i++)
        {
            tan[i] = ComputeTangent(n[i]);
        }
    }
    */

    void ComputeMeshNormal(Vector3[] v, Vector3[] n)
    {
        int x, y, index;
        for (y = 0; y < (mResolution - 1); y++)
            for (x = 0; x < (mResolution-1); x++)
            {
                int count = 0;
                index = PosToVertexIndex(x, y);

                // 1. get the ul of (x, y)
                n[index] = TriangleNormal(v, x, y, kLowerTriangleFlag);
                n[index] += TriangleNormal(v, x, y, kUpperTriangleFlag);
                count += 2;

                if (x > 0)
                {
                    n[index] += TriangleNormal(v, x - 1, y, kLowerTriangleFlag);
                    count++;
                }
                if (y > 0)
                {
                    n[index] += TriangleNormal(v, x, y - 1, kUpperTriangleFlag);
                    count++;
                }
                if (count == 4) // this means both x and y > 0
                {
                    n[index] += TriangleNormal(v, x - 1, y - 1, kLowerTriangleFlag);
                    n[index] += TriangleNormal(v, x - 1, y - 1, kUpperTriangleFlag);
                    count += 2;
                }
                n[index].Normalize();
            } 

        // now the edge cases:
        //   1. Bottom-Right (only one triangle) (Resolution - 1, 0)
        //   2. Top-Left (only one triangles)
        index = mResolution - 1;  // Bottom Right
        n[index] = TriangleNormal(v, mResolution - 2, 0, kLowerTriangleFlag);
        n[index].Normalize();

        index = (mResolution - 1) * mResolution; // Top-Left
        n[index] = TriangleNormal(v, 0, mResolution - 2, kUpperTriangleFlag);
        n[index].Normalize();

        // now the vertices at right edges (does not incldue the bottom-right vertex)
        //    for this last column: (Resolution-1, y)
        //    Triangles are the two from (Resolution-2, y-1) AND lower from the y above
        x = mResolution - 1;  // x is always Resoution - 1
        for (y = 1; y < mResolution; y++)
        {
            // we are always interested in triangles from (x-1, y-1)
            index = PosToVertexIndex(x, y);
            n[index] = TriangleNormal(v, x - 1, y - 1, kLowerTriangleFlag);
            n[index] += TriangleNormal(v, x - 1, y - 1, kUpperTriangleFlag);
            if (y < (mResolution -1))
                n[index] += TriangleNormal(v, x - 1, y, kLowerTriangleFlag);
            n[index].Normalize();
        }

        // now the top row
        y = mResolution - 1;
        for (x = 1; x < mResolution - 1; x++) // does not include the two corners (done with those)
        {
            index = PosToVertexIndex(x, y);
            n[index] = TriangleNormal(v, x - 1, y - 1, kLowerTriangleFlag);
            n[index] += TriangleNormal(v, x - 1, y - 1, kUpperTriangleFlag);
            n[index] += TriangleNormal(v, x, y - 1, kUpperTriangleFlag);
            n[index].Normalize();
        }
    }
}
