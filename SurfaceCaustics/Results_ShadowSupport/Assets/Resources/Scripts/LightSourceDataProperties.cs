using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightSourceDataProperties
{
    public RenderTexture RefractionPositionTexture;

    public RenderTexture RefractionNormalTexture;

    public RenderTexture ReceivingPositionTexture;

    public RenderTexture RefractionDistanceTexture;

    public RenderTexture RefractionFluxTexture;

    public RenderTexture RefractionColorTexture;

    public RenderTexture FinalColorTexture;

    public RenderTexture DebugSplatPosTexture;

    public int LightSourceID;
}