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
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            // Variables set globally from the CPU
            sampler2D _CausticTexture;
            sampler2D _SpecularPosTexture;
            sampler2D _CausticFluxTexture;
            sampler2D _DrewTest;
            sampler2D _CausticMapTexture;
            sampler2D _CausticDistanceTexture;

            float4x4 _LightViewProjectionMatrix;
            float _IlluminationDistance;
            float _GlobalAbsorbtionCoefficient;
            float _DebugFlux;
            float3 _DiffuseObjectPos;

            float3 _LightWorldPosition;

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

            /*
            * Determine if the world position (of the receiving object) is hit by any refracted
            * light ray
            */
            bool SpecularSeesPosition(float3 worldPos)
            {
                float2 tc = GetCoordinatesForSpecularTexture(worldPos);
                float4 fluxVals = tex2D(_DrewTest/*_CausticMapTexture*/, tc);
                return fluxVals.a == 1;
            }

            /*
            * Get the amount of flux that has accumulated onto the provided receiving position
            */
            float GetFlux(float3 worldPos)
            {
                float2 tc = GetCoordinatesForSpecularTexture(worldPos);
                float4 fluxVals = tex2D(_CausticFluxTexture, tc);
                return fluxVals.x;
            }

            float GetDistance(float3 worldPos)
            {
                float2 tc = GetCoordinatesForSpecularTexture(worldPos);
                float4 distanceVals = tex2D(_CausticDistanceTexture, tc);
                return distanceVals.x;
            }

            v2f vert (appdata v)
            {
                v2f o;
                float3 worldPos = mul(UNITY_MATRIX_M, v.vertex);
                float3 worldNormal = mul(transpose(unity_WorldToObject), v.normal);

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = worldPos;
                return o;
            }

            
            fixed4 frag (v2f i) : SV_Target
            {
                float absorbtionCoef = pow(10, -5);
            float flux = _DebugFlux; //GetFlux(i.worldPos);
                float d = GetDistance(i.worldPos);
                float finalIntensity = flux * exp((-_GlobalAbsorbtionCoefficient/*absorbtionCoef*/ * d));
                fixed4 col = tex2D(_MainTex, i.uv);

                if (SpecularSeesPosition(i.worldPos))
                {
                    return col + finalIntensity;
                }

                return col;
            }
            ENDCG
        }
    }
}
