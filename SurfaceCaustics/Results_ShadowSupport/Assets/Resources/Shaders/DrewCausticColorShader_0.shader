Shader "Unlit/DrewCausticColorShader_0"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "SpecularObj" = "1" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "CausticColorFunctions.cginc"

            //Light specific parameters
            sampler2D _ReceivingPosTexture_0;
            float4x4 _LightViewProjectionMatrix_0;
            float3 _LightWorldPosition_0;

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _ObjectRefractionIndex;
            int _NumProjectedVerticies;

            v2f vert (appdata v)
            {
                float2 uv = TRANSFORM_TEX(v.uv, _MainTex);
                return SharedColorVertexShader(
                    v,
                    _LightViewProjectionMatrix_0,
                    _LightWorldPosition_0,
                    _ObjectRefractionIndex,
                    uv,
                    _ReceivingPosTexture_0,
                    _NumProjectedVerticies
                );
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return SharedColorFragmentShader(i, _MainTex);
            }
            ENDCG
        }
    }
}
