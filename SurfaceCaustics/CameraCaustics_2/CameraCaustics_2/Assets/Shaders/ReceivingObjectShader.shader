Shader "Unlit/ReceivingObjectShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "ReceivingObject"="1" }
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
            sampler2D _Pass1Data;
            float4x4 _LightViewProjectionMatrix;
            float4 _MainTex_ST;
            float _AbsorbtionCoefficient;

            float2 GetCoordinatesForSpecularTexture(float3 worldPos)
            {
                float4 texPt = mul(_LightViewProjectionMatrix, float4(worldPos, 1));
                float2 tc = 0.5 * texPt.xy / texPt.w + float2(0.5, 0.5);
                return tc;
            }

            float GetFlux(float3 worldPos)
            {
                float2 tc = GetCoordinatesForSpecularTexture(worldPos);
                float4 fluxVals = tex2D(_Pass1Data, tc);
                return fluxVals.x;
            }

            float GetDistance(float3 worldPos)
            {
                float2 tc = GetCoordinatesForSpecularTexture(worldPos);
                float4 fluxVals = tex2D(_Pass1Data, tc);
                return fluxVals.y;
            }

            v2f vert (appdata v)
            {
                v2f o;
                float3 vertexWorldPos = mul(UNITY_MATRIX_M, v.vertex);
                
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = vertexWorldPos;

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float flux = GetFlux(i.worldPos);
                float dist = GetDistance(i.worldPos);
                float finalIntensity = flux * exp((-_AbsorbtionCoefficient * dist));
                fixed4 lightColor = fixed4(1, 1, 1, 1);
                float computedColorValue = finalIntensity * lightColor ;


                return fixed4(computedColorValue, computedColorValue, computedColorValue, 1);
                
                /*if (flux > 0)
                {
                    return fixed4(0, 1, 0, 1);
                }

                return fixed4(1, 0, 0, 1);*/
                /*return fixed4(0, 0, 0, 1);*/
            }
            ENDCG
        }
    }
}
