/*
* WARNING: This shader is depracated
*/
Shader "Unlit/CausticIntensityShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
        SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4x4 _LightViewProjectionMatrix;
            sampler2D _CausticFluxTexture;
            sampler2D _CausticMapTexture;
            RWStructuredBuffer<float> TotalFluxBuffer;

            float4 GetCausticIntensity(float3 worldPos)
            {
                float4 texPt = mul(_LightViewProjectionMatrix, float4(worldPos, 1));
                float2 tc = 0.5 * texPt.xy / texPt.w + float2(0.5, 0.5);
                float4 causticIntensity = tex2D(_CausticFluxTexture, tc);
                return causticIntensity;
            }

            v2f vert (appdata v)
            {
                v2f o;
                float3 worldPos = mul(UNITY_MATRIX_M, v.vertex);

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = worldPos;

                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                // sample the texture
                float4 col = tex2D(_MainTex, i.uv);
                float4 causticReceiverWorldPos = tex2D(_CausticMapTexture, i.uv);
                float4 causticFlux = tex2D(_CausticFluxTexture, i.uv);
                return col;
            }
            ENDCG
        }
    }
}
