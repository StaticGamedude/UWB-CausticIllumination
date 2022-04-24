Shader "Unlit/CausticFinalShader"
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
                float3 splatPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _ObjectRefractionIndex;
            // Variables set globally from the CPU
            sampler2D _DrewCausticColor;
            sampler2D _CausticTexture;
            sampler2D _SpecularPosTexture;
            sampler2D _CausticFluxTexture;
            sampler2D _DrewTest;
            sampler2D _CausticMapTexture;
            sampler2D _CausticDistanceTexture;
            sampler2D _CausticColorMapTexture;

            float _GlobalAbsorbtionCoefficient;
            fixed4 _DebugLightColor;
            float _LightIntensity;

            /*
           * Given the world position of the receiving object, get the texture
           * coordinates that can be used to map into a caustic texture.
           */
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

            fixed4 GetCausticColor(float3 worldPos)
            {
                float2 tc = GetCoordinatesForSpecularTexture(worldPos);
                fixed4 causticColor = tex2D(_DrewCausticColor, tc);
                return causticColor;
            }

            v2f vert (appdata v)
            {
                v2f o;
                float3 worldPos = mul(UNITY_MATRIX_M, v.vertex);
                float3 worldNormal = normalize(mul(transpose(unity_WorldToObject), v.normal));
                float3 refractedDirection = RefractRay(worldPos, worldNormal, _ObjectRefractionIndex);
                float3 estimatedPosition = VertexEstimateIntersection(worldPos, refractedDirection, _ReceivingPosTexture);

                o.vertex = mul(UNITY_MATRIX_VP, float4(estimatedPosition, 1));
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.splatPos = estimatedPosition;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float flux = GetFlux(i.splatPos); //_DebugFlux;
                float d = GetDistance(i.splatPos);
                float finalIntensity = flux * exp((-_GlobalAbsorbtionCoefficient * d));
                fixed4 causticColor = GetCausticColor(i.splatPos);

                return finalIntensity * _DebugLightColor * causticColor * _LightIntensity;
            }
            ENDCG
        }
    }
}
