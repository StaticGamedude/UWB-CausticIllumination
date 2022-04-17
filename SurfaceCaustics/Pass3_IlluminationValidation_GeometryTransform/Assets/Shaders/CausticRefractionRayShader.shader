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

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                float3 worldPos = mul(UNITY_MATRIX_M, v.vertex);
                float3 worldNormal = normalize(mul(transpose(unity_WorldToObject), v.normal));
                float3 refractedDirection = RefractRay(worldPos, worldNormal);
                float3 estimatedPosition = VertexEstimateIntersection(worldPos, refractedDirection, _ReceivingPosTexture);
                
                if (_Debug_TransformSpecularGeometry == 1)
                {
                    o.vertex = mul(UNITY_MATRIX_VP, float4(estimatedPosition, 1));
                }
                else
                {
                    o.vertex = UnityObjectToClipPos(v.vertex);
                }
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
