Shader "Unlit/SpecularReceivingObject"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "SpecularReceiver"="1"}
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
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD1;
                bool seePos : TEXCOORD2;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            // Variables set globally from the CPU
            sampler2D _CausticTexture;
            sampler2D _SpecularPosTexture;
            sampler2D _CausticFluxTexture;
            sampler2D _DrewTest;
            sampler2D _CausticDistanceTexture;

            float4x4 _LightViewProjectionMatrix;
            float _IlluminationDistance;
            float _GlobalAbsorbtionCoefficient;
            float _DebugFlux;
            float3 _DiffuseObjectPos;

            float3 _LightWorldPosition;


            float2 GetCoordinatesForSpecularTexture(float3 worldPos)
            {
                float4 texPt = mul(_LightViewProjectionMatrix, float4(worldPos, 1));
                float2 tc = 0.5 * texPt.xy / texPt.w + float2(0.5, 0.5);
                return tc;
            }

            float GetFlux(float3 worldPos)
            {
                float2 tc = GetCoordinatesForSpecularTexture(worldPos);
                float4 fluxVals = tex2D(_DrewTest, tc);
                return fluxVals.x;
            }

            float GetDistance(float3 worldPos)
            {
                float2 tc = GetCoordinatesForSpecularTexture(worldPos);
                float4 distanceVals = tex2D(_CausticDistanceTexture, tc);
                return distanceVals.x;
            }

            bool SpecularSeesPosition(float3 worldPos)
            {
                float2 tc = GetCoordinatesForSpecularTexture(worldPos);
                float4 fluxVals = tex2D(_DrewTest, tc);
                return fluxVals.a == 1;
            }

            bool SpecularSeesPosition_2(float3 worldPos)
            {
                float2 tc = GetCoordinatesForSpecularTexture(worldPos);
                float4 fluxVals = tex2Dlod(_DrewTest, float4(tc, 1, 1));
                return fluxVals.a == 1;
            }

            v2f vert (appdata v)
            {
                v2f o;
                float3 worldPos = mul(UNITY_MATRIX_M, v.vertex);
                float3 worldNormal = mul(transpose(unity_WorldToObject), v.normal);

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = worldPos;
                o.seePos = SpecularSeesPosition_2(worldPos);
                return o;
            }

            
            fixed4 frag (v2f i) : SV_Target
            {
                //float absorbtionCoef = pow(10, -5);
                float flux = _DebugFlux; //GetFlux(i.worldPos);
                float d = GetDistance(i.worldPos);
                float finalIntensity = flux * exp((-_GlobalAbsorbtionCoefficient * d));
                fixed4 col = tex2D(_MainTex, i.uv);

                if (!SpecularSeesPosition(i.worldPos)/*i.seePos*/)
                {
                    return col;
                    /*return fixed4(1, 0, 0, 1);*/
                }
                else
                {
                    return col + finalIntensity;
                    //return fixed4(0, 1, 0, 1);
                }

                //return col;
            }
            ENDCG
        }
    }
}
