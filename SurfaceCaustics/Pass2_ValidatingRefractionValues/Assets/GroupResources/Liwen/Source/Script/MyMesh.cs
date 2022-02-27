using UnityEngine;
using System.Collections;

// Read about mesh: https://docs.unity3d.com/ScriptReference/Mesh.html
//
public partial class MyMesh : MonoBehaviour {
    public float kInitSize = 20f;

    protected void Awake()
    {
        InitializeTextureTransform();
    }

    // Use this for initialization
    protected void Start()
    {
        //mShowMeshPts = new GameObject("ShowMesh");
        //mShowNormals = new GameObject("ShowNormals");
        //mShowMeshPts.transform.SetParent(transform);
        //mShowNormals.transform.SetParent(transform);

        transform.localScale = new Vector3(kInitSize, kInitSize, kInitSize);

        BuildNewMesh();

        //SetShowMarkers(false);
        //SetShowNormals(false);

        Mesh m = GetComponent<MeshFilter>().mesh;
        Vector3[] v = m.vertices;
        Vector3[] n = m.normals;
        Vector2[] uv = m.uv;
        int[] t = m.triangles;

        // OK, this is stupid and very inefficient, but ... for now
        ComputeMeshNormal(v, n);
        //ComputeTangent(n, tan);
        //UpdateDrawElements(v, n);
        UpdateTextureTransform(uv);

        m.Clear();
        m.vertices = v;
        m.normals = n;
        m.uv = uv;
        m.triangles = t;

        //Debug.Log(v.Length + " " + n.Length);
        ComputeTangent(m);
    }

    // Update is called once per frame
    protected void Update()
    {
    }
}
