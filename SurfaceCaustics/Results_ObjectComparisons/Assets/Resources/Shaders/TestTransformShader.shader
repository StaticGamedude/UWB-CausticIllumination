Shader "Unlit/TestTransformShader"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        //_ObjectRefractionIndex("Refraction Index", Float) = 1.0
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque"}
        LOD 100

        Pass
        {
            CGPROGRAM
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
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _ObjectRefractionIndex;

            v2f vert(appdata v)
            {
                v2f o;
                float3 specularVertexWorldPos = mul(UNITY_MATRIX_M, v.vertex);
                float3 specularVertexWorldNormal = normalize(mul(transpose(unity_WorldToObject), v.normal));
                float3 refractedDirection = RefractRay(specularVertexWorldPos, specularVertexWorldNormal, _ObjectRefractionIndex);
                float3 estimatedPosition = VertexEstimateIntersection(specularVertexWorldPos, refractedDirection, _ReceivingPosTexture);

                o.vertex = mul(UNITY_MATRIX_VP, float4(estimatedPosition, 1));
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                return float4(1, 0, 0, 1);
            }

            ENDCG
        }
    }
}
