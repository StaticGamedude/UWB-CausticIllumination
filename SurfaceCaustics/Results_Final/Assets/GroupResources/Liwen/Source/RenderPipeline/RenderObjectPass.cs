using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class RenderObjectPass {
    private ShaderTagId _shaderTag;
    private bool _isTransparent = false;
    private bool _perObjectLight = false;

    public RenderObjectPass(bool transparent, string lightModeTagId, bool perObjectLight)
    {
        _shaderTag = new ShaderTagId(lightModeTagId);
        _isTransparent = transparent;
        _perObjectLight = perObjectLight;
    }

    public RenderObjectPass(bool transparent, string lightModeTagId) : this(transparent, lightModeTagId, true)
    {

    }

    public RenderObjectPass(bool transparent) : this(transparent, "XForwardBase")
    {
    }

    public void Execute(ScriptableRenderContext context, Camera camera, ref CullingResults cullingResults)
    {
        var drawSetting = CreateDrawSettings(camera);
        var filterSetting = new FilteringSettings(_isTransparent ? RenderQueueRange.transparent : RenderQueueRange.opaque);
        context.DrawRenderers(cullingResults, ref drawSetting, ref filterSetting);
/*        var drawSetting = new DrawingSettings();
        var filterSetting = new FilteringSettings();*/
        //context.DrawRenderers(cullingResults, ref drawSetting, ref filterSetting);
    }

    private DrawingSettings CreateDrawSettings(Camera camera)
    {
        ShaderTagId A = new ShaderTagId("ObjectA");
        ShaderTagId B = new ShaderTagId("ObjectB");
        ShaderTagId C = new ShaderTagId("ObjectC");

        var sortingSetting = new SortingSettings(camera);
        sortingSetting.criteria = _isTransparent ? SortingCriteria.CommonTransparent : SortingCriteria.CommonOpaque;
        var drawSetting = new DrawingSettings(A, sortingSetting);
        drawSetting.perObjectData |= PerObjectData.LightData;
        drawSetting.perObjectData |= PerObjectData.LightIndices;

        // Set Multi-Pass
        drawSetting.SetShaderPassName(2, A);
        drawSetting.SetShaderPassName(1, B);
        drawSetting.SetShaderPassName(0, C);

        return drawSetting;
    }
}
