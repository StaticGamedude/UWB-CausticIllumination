using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class DeferredLightingPass
{

    private CommandBuffer _commandbuffer = new CommandBuffer();

    [System.NonSerialized]
    private Material _lightPassMat;
    private Mesh _fullScreenMesh;
    public DeferredLightingPass()
    {
        _commandbuffer.name = "DeferredLightingPass";
    }
    public void Execute(ScriptableRenderContext context)
    {
        if (!_lightPassMat)
        {
            _lightPassMat = new Material(Shader.Find("Unlit/CombineShader"));
            _lightPassMat.DisableKeyword("_RECEIVE_SHADOWS_OFF");
        }
        if (!_fullScreenMesh)
        {
            _fullScreenMesh = CreateFullscreenMesh();
        }
        _commandbuffer.Clear();
        _commandbuffer.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);
        _commandbuffer.DrawMesh(_fullScreenMesh, Matrix4x4.identity, _lightPassMat, 0, 0);
        context.ExecuteCommandBuffer(_commandbuffer);
    }

    public static Mesh CreateFullscreenMesh()
    {
        Vector3[] positions =
        {
                new Vector3(-1.0f,  -1.0f, 0.0f),
                new Vector3(1.0f, -1.0f, 0.0f),
                new Vector3(-1.0f,  1.0f, 0.0f),
                new Vector3(1.0f, 1.0f, 0.0f),
            };
        Vector2[] uvs = {
                new Vector2(0,0),
                new Vector2(1,0),
                new Vector2(0,1),
                new Vector2(1,1),
            };
        int[] indices = { 0, 2, 1, 1, 2, 3 };
        Mesh mesh = new Mesh();
        mesh.indexFormat = IndexFormat.UInt16;
        mesh.vertices = positions;
        mesh.triangles = indices;
        mesh.uv = uvs;
        return mesh;
    }

}
