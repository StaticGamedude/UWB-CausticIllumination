Shader "Unlit/CausticRefractionRayShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "SpecularObj" = "1" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma target 5.0
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "CommonFunctions.cginc"

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
                float3 worldRefractedRayDirection : TEXCOORD2;
            };

            //Light specific parameters
            sampler2D _ReceivingPosTexture;
            float4x4 _LightViewProjectionMatrix;
            float3 _LightWorldPosition;

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _ObjectRefractionIndex;

            v2f vert (appdata v)
            {
                v2f o;
                float3 worldPos = mul(UNITY_MATRIX_M, v.vertex);
                float3 worldNormal = normalize(mul(transpose(unity_WorldToObject), v.normal));
                float3 refractedDirection = RefractRay(_LightWorldPosition, worldPos, worldNormal, _ObjectRefractionIndex);
                float3 estimatedPosition = VertexEstimateIntersection(_LightViewProjectionMatrix, worldPos, refractedDirection, _ReceivingPosTexture);

                o.vertex = mul(UNITY_MATRIX_VP, float4(estimatedPosition, 1));
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldRefractedRayDirection = refractedDirection;
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                return float4(i.worldRefractedRayDirection, 1);
            }
            ENDCG
        }
    }
}
