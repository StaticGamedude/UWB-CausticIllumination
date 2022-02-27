Shader "Unlit/CausticFluxShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma target 5.0
            #pragma vertex vert
            #pragma fragment frag
            

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float flux : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            // Variables set globally from the CPU
            int _NumProjectedVerticies;
            sampler2D _CausticTexture;
            float4x4 _LightViewProjectionMatrix;
            float3 _LightWorldPosition;

            float GetFluxContribution(float visibleSurfaceArea, float3 worldPosition, float3 worldNormal)
            {
                float3 incidentLightVector = normalize(_LightWorldPosition - worldPosition);
                return (1 / visibleSurfaceArea) * dot(worldNormal, incidentLightVector);
            }

            v2f vert (appdata v)
            {
                v2f o;
                float3 worldPos = mul(UNITY_MATRIX_M, v.vertex);
                float3 worldNormal = mul(transpose(unity_WorldToObject), v.normal);
                float3 flux = GetFluxContribution(_NumProjectedVerticies, worldPos, worldNormal);

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.flux = flux;
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                return float4(i.flux, i.flux, i.flux, 1);
            }

            ENDCG
        }
    }
}
