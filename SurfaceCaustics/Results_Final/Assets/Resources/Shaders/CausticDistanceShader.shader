/*
* !!!!WARNING: This shader is now depricated. The distance value is now calcuated in the Flux shaders (CausticFluxFunctions.cginc)
* --------------------------------------------------------------------------------------------------------------------------------------------
* Shader responsible for capturing the distance between each vertex and the splat position
*/

Shader "Unlit/CausticDistanceShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "SpecularObj" = "1"  }
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
                float4 normal: NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float distance : TEXCOORD1;
            };

            //Light specific parameters
            sampler2D _ReceivingPosTexture;
            float4x4 _LightViewProjectionMatrix;
            float3 _LightWorldPosition;
            float3 _LightCam_Forward_0;
            int _LightIsDirectional_0;

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _ObjectRefractionIndex;

            v2f vert (appdata v)
            {
                v2f o;
                float3 worldPos = mul(UNITY_MATRIX_M, v.vertex);
                float3 worldNormal = normalize(mul(transpose(unity_WorldToObject), v.normal));
                float3 refractedDirection = RefractRay(_LightWorldPosition, _LightCam_Forward_0, _LightIsDirectional_0, worldPos, worldNormal, _ObjectRefractionIndex);
                float3 estimatedPosition = VertexEstimateIntersection(_LightViewProjectionMatrix, worldPos, refractedDirection, _ReceivingPosTexture);

                o.vertex = mul(UNITY_MATRIX_VP, float4(estimatedPosition, 1));
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.distance = distance(worldPos, estimatedPosition);
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                float isVisible = 0;
                if (col.r != 0 || col.g != 0 || col.b != 0)
                {
                    return fixed4(i.distance, i.distance, i.distance, 1);
                }

                return fixed4(0, 0, 0, 0);
            }
            ENDCG
        }
    }
}
