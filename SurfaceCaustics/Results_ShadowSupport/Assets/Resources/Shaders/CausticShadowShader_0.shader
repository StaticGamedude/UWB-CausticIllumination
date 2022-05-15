Shader "Unlit/CausticShadowShader_0"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
    }
        SubShader
    {
        Tags { "RenderType" = "Opaque" "SpecularObj" = "1" }
        LOD 100

        Pass
        {
            Blend One One

            CGPROGRAM
            #pragma target 5.0
            #pragma vertex vert
            #pragma fragment frag

            #include "CausticFluxFunctions.cginc"

            //Light specific parameters
            sampler2D _ReceivingPosTexture_0;
            float4x4 _LightViewProjectionMatrix_0;
            float3 _LightWorldPosition_0;

            sampler2D _MainTex;
            float4 _MainTex_ST;

            // Variables set globally from the CPU
            int _NumProjectedVerticies;
            float _DebugFluxMultiplier;
            float _ObjectRefractionIndex;

            v2f vert(appdata v)
            {
                float2 uv = TRANSFORM_TEX(v.uv, _MainTex);
                return SharedFluxVertexShader(
                        v,
                        _LightViewProjectionMatrix_0,
                        _LightWorldPosition_0,
                        1,
                        uv,
                        _ReceivingPosTexture_0,
                        _NumProjectedVerticies
                );
            }

            float4 frag(v2f i) : SV_Target
            {
                return SharedFluxFragmentShader(i, _MainTex, _DebugFluxMultiplier);
            }

            ENDCG
        }
    }
}