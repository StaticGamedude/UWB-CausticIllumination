using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayMeshCount : MonoBehaviour
{
    public int VertexCount;
    // Start is called before the first frame update
    void Start()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        this.VertexCount = mesh.vertexCount;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
