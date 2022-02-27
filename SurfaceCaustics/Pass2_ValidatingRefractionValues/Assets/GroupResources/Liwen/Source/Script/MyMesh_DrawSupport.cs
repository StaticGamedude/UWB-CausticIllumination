using UnityEngine;
using System.Collections;

// Read about mesh: https://docs.unity3d.com/ScriptReference/Mesh.html
//
public partial class MyMesh : MonoBehaviour {

    Vector3 kMarkerSize = new Vector3(0.1f, 0.1f, 0.1f);
    Color kMarkerColor = Color.white;

    const float kNormalHeight = 0.5f;
    const float kNormalRadius = 0.05f;
    Vector3 kNormalSize = new Vector3(kNormalRadius, kNormalHeight, kNormalRadius);
    Color kNormalColor = Color.white;


    private GameObject mShowMeshPts;  // for showing the sphere positions
    private GameObject mShowNormals;  // for showing the normals

    protected GameObject[] mMarkers = null;
    protected LineSegment[] mNormals = null;
    private const string kSelectionSphereName = "SelectionSphere";
    private const int kSelectionLayer = 20;

    void CreateDrawSupport()
    {
        if (mMarkers != null)
        {
            // assume the same for normal
            for (int i = 0; i < mMarkers.Length; i++)
            {
                Destroy(mNormals[i].gameObject);
                Destroy(mMarkers[i]);
            }
        }
        mMarkers = new GameObject[mNumVertices];
        mNormals = new LineSegment[mNumVertices];
        // Crete the vertex markers and normal vectors
        for (int i = 0; i < mNumVertices; i++)
        {
            mMarkers[i] = CreateMarker();
            mNormals[i] = CreateNormal();
        }
    }

    // Update functions
    void UpdateDrawElements(Vector3[] v, Vector3[] n)
    {
        // check to make sure we need to do the work
        if (mShowMeshPts.activeSelf) {
            for (int i = 0; i < mNumVertices; i++)
                v[i] = mMarkers[i].transform.localPosition;
        }

        if (mShowNormals.activeSelf)
        {
            for (int i = 0; i < mNumVertices; i++)
                mNormals[i].SetEndPoints(v[i], v[i] + kNormalHeight * n[i]);
        }
    }
       
    // Creation functions ...
    GameObject CreateMarker()
    {
        GameObject o = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        o.name = kSelectionSphereName;
        o.layer = kSelectionLayer;
        o.transform.SetParent(mShowMeshPts.transform);
        o.transform.localScale = kMarkerSize;
        o.GetComponent<Renderer>().material.color = kMarkerColor;
        return o;
    }

    LineSegment CreateNormal()
    {
        GameObject o = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        LineSegment l = o.AddComponent<LineSegment>();
        l.SetWidth(kNormalRadius);
        o.transform.SetParent(mShowNormals.transform);
        o.GetComponent<Renderer>().material.color = kNormalColor;
        return l;
    }
}
