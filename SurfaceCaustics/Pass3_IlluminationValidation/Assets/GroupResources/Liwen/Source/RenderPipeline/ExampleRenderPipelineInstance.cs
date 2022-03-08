using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine.Experimental.Rendering;

public class RenderPipelineInstance : RenderPipeline
{
    protected ExampleRenderPipelineAsset _setting;
    protected CommandBuffer _commandbuffer;

    private List<RenderTexture> _GBuffers = new List<RenderTexture>();
    private RenderTargetIdentifier[] _GBufferRTIs;
    private int[] _GBufferNameIDs = {
            ShaderConstants.GBuffer0,
            ShaderConstants.GBuffer1,
            ShaderConstants.GBuffer2,
            ShaderConstants.GBuffer3,
        };
    private RenderTextureFormat[] _GBufferFormats = {
            RenderTextureFormat.ARGB32,
            RenderTextureFormat.ARGB32,
            RenderTextureFormat.ARGB32,
            RenderTextureFormat.ARGB32
        };

    private RenderTexture _depthTexture;
    private const string LightModeId = "Deferred";
    private const string LightModeId2 = "Deferred2";
    private RenderObjectPass _opaquePass = new RenderObjectPass(false, LightModeId, false);
    private RenderObjectPass _opaquePass2 = new RenderObjectPass(false, LightModeId2, false);
    private DeferredLightingPass _deferredLightingPass = new DeferredLightingPass();

    public RenderPipelineInstance(ExampleRenderPipelineAsset setting)
    {
        GraphicsSettings.useScriptableRenderPipelineBatching = true;
        _setting = setting;
        _commandbuffer = new CommandBuffer();
        _commandbuffer.name = "RP";
    }

    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        // pipeline begin
        foreach (var camera in cameras)
        {
            RenderPerCamera(context, camera);
        }
        context.Submit();
        // pipeline ended
    }

    private void RenderPerCamera(ScriptableRenderContext context, Camera camera)
    {
        context.SetupCameraProperties(camera);

        camera.TryGetCullingParameters(out var cullingParams);
        var cullingResults = context.Cull(ref cullingParams);
        //context.DrawSkybox(camera);
        this.OnPostCameraCulling(context, camera, ref cullingResults);

        //context.DrawSkybox(camera);
    }

    protected void OnPostCameraCulling(ScriptableRenderContext context, Camera camera, ref CullingResults cullingResults)
    {
        //var cameraDesc = Utils.GetCameraRenderDescription(camera, _setting);
        this.ConfigMRT(context, camera);
        //_opaquePass.Execute(context, camera, ref cullingResults);
        NativeArray<VisibleLight> visibleLights = cullingResults.visibleLights;
        if (visibleLights != null && visibleLights.Length > 0)
        {
            VisibleLight mainLight = visibleLights[0];
            Vector3 mainLightPos = mainLight.light.gameObject.transform.position;
            Vector4 mainLightForward = -(Vector4)mainLight.light.gameObject.transform.forward;
            //Debug.Log(mainLightPos);
            _commandbuffer.SetGlobalVector("LightPosition", mainLightPos);
            _commandbuffer.SetGlobalVector("LightDirection", mainLightForward);
        }
        _opaquePass2.Execute(context, camera, ref cullingResults);

        // stencil test color buffere below
        /*        RenderTexture stencilBufferToColor = new RenderTexture(Screen.width, Screen.height, 24);
                Graphics.SetRenderTarget(stencilBufferToColor);

                GL.Clear(true, true, new Color(0, 0, 0, 0));
                _commandbuffer.SetTexture("_StencilBufferToColor", stencilBufferToColor);*/

        // combine
        _commandbuffer.Clear();
        _commandbuffer.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
        context.ExecuteCommandBuffer(_commandbuffer);


        _deferredLightingPass.Execute(context);
    }

    private void GenerateStencilBufferColor(ScriptableRenderContext context, Camera camera)
    {
        //RenderTexture stencilBufferToColor = new RenderTexture(camera.pixelWidth, camera.pixelHeight, 24);
        //OutlinePostProcessByStencilMat.SetTexture("_StencilBufferToColor", stencilBufferToColor);
    }

    private void ConfigMRT(ScriptableRenderContext context, Camera camera)
    {
        this.AcquireGBuffersIfNot(context, camera);
        this.AcquireDepthBuffer(context, camera);
        //todo: _depthTexture
        _commandbuffer.Clear();
        _commandbuffer.SetRenderTarget(_GBufferRTIs, _depthTexture);
        //_commandbuffer.ClearRenderTarget(true, true, Color.clear);
        _commandbuffer.ClearRenderTarget(true, true, camera.backgroundColor);
        context.ExecuteCommandBuffer(_commandbuffer);
    }

    private void AcquireDepthBuffer(ScriptableRenderContext context, Camera camera)
    {
/*        RenderTextureDescriptor depthDesc = new RenderTextureDescriptor(camera.pixelWidth, camera.pixelHeight, RenderTextureFormat.Depth, 32, 1);
        RenderTexture _depthTexture = RenderTexture.GetTemporary(depthDesc);
        _depthTexture.Create();*/

        if (_depthTexture)
        {
            if (_depthTexture.width != camera.pixelWidth || _depthTexture.height != camera.pixelHeight)
            {
                RenderTexture.ReleaseTemporary(_depthTexture);
                _depthTexture = null;
            }
        }
        if (!_depthTexture)
        {
            RenderTextureDescriptor depthDesc = new RenderTextureDescriptor(camera.pixelWidth, camera.pixelHeight, RenderTextureFormat.Depth, 32, 1);
            _depthTexture = RenderTexture.GetTemporary(depthDesc);
            _depthTexture.Create();
            _commandbuffer.Clear();
            _commandbuffer.SetGlobalTexture("_Depth", _depthTexture);
            context.ExecuteCommandBuffer(_commandbuffer);
        }
    }

    private void AcquireGBuffersIfNot(ScriptableRenderContext context, Camera camera)
    {
        if (_GBuffers.Count > 0)
        {
            var g0 = _GBuffers[0];
            if (g0.width != camera.pixelWidth || g0.height != camera.pixelHeight)
            {
                this.ReleaseGBuffers();
            }
        }
        if (_GBuffers.Count == 0)
        {
            _commandbuffer.Clear();
            _GBufferRTIs = new RenderTargetIdentifier[4];
            for (var i = 0; i < 4; i++)
            {
                RenderTextureDescriptor descriptor = new RenderTextureDescriptor(camera.pixelWidth, camera.pixelHeight, _GBufferFormats[i], 0, 1)
                {
                    stencilFormat = GraphicsFormat.R8_UInt, //Enable stencil buffer

                };
                var rt = RenderTexture.GetTemporary(descriptor);
                rt.filterMode = FilterMode.Bilinear;
                rt.Create();
                _GBuffers.Add(rt);
                _commandbuffer.SetGlobalTexture(_GBufferNameIDs[i], rt);
                _GBufferRTIs[i] = rt;
            }
            context.ExecuteCommandBuffer(_commandbuffer);
        }
    }

    private void ReleaseGBuffers()
    {
        if (_GBuffers.Count > 0)
        {
            foreach (var g in _GBuffers)
            {
                if (g)
                {
                    RenderTexture.ReleaseTemporary(g);
                }
            }
            _GBuffers.Clear();
            _GBufferRTIs = null;
        }
    }


    public static class ShaderConstants
    {
        public static readonly int GBuffer0 = Shader.PropertyToID("_GBuffer0");
        public static readonly int GBuffer1 = Shader.PropertyToID("_GBuffer1");
        public static readonly int GBuffer2 = Shader.PropertyToID("_GBuffer2");
        public static readonly int GBuffer3 = Shader.PropertyToID("_GBuffer3");

        public static readonly int GBuffer7 = Shader.PropertyToID("_GBuffer7");

        /*public static readonly int CameraColorTexture = Shader.PropertyToID("_CameraColorTexture");

        public static readonly int CameraDepthTexture = Shader.PropertyToID("_XDepthTexture");
        public static readonly int DeferredDebugMode = Shader.PropertyToID("_DeferredDebugMode");

        public static readonly int TileCullingIntersectAlgroThreshold = Shader.PropertyToID("_TileCullingIntersectAlgroThreshold");*/
    }
}