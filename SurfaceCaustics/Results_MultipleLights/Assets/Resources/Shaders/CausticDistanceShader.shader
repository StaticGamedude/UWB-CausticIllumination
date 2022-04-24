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

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _ObjectRefractionIndex;

            v2f vert (appdata v)
            {
                v2f o;
                float3 worldPos = mul(UNITY_MATRIX_M, v.vertex);
                float3 worldNormal = normalize(mul(transpose(unity_WorldToObject), v.normal));
                float3 refractedDirection = RefractRay(worldPos, worldNormal, _ObjectRefractionIndex);
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
