Shader "Unlit/CausticFluxShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "SpecularObj"="1" }
        LOD 100

        Pass
        {
            Blend One One

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
                float flux : TEXCOORD1;
                float distance : TEXCOORD2;
            };

            //Light specific parameters
            sampler2D _ReceivingPosTexture;
            float4x4 _LightViewProjectionMatrix;
            float3 _LightWorldPosition;

            sampler2D _MainTex;
            float4 _MainTex_ST;

            // Variables set globally from the CPU
            int _NumProjectedVerticies;
            float _DebugFlux;
            float _DebugFluxMultiplier;
            float _ObjectRefractionIndex;

            float GetFluxContribution(float visibleSurfaceArea, float3 worldPosition, float3 worldNormal)
            {
                float3 incidentLightVector = normalize(_LightWorldPosition - worldPosition);

                //Because of the possibility of negative angles between the normal and light vector, we clamp the value
                return (1 / visibleSurfaceArea) * max(dot(worldNormal, incidentLightVector), 0.01); 
            }

            v2f vert (appdata v)
            {
                v2f o;
                float3 worldPos = mul(UNITY_MATRIX_M, v.vertex);
                float3 worldNormal = normalize(mul(transpose(unity_WorldToObject), v.normal));
                float3 estimatedPosition = GetEstimatedSplatPosition( 
                                            _LightViewProjectionMatrix,
                                            _LightWorldPosition,
                                            _ObjectRefractionIndex,
                                            worldPos,
                                            worldNormal,
                                            _ReceivingPosTexture);
                float3 flux = GetFluxContribution(_NumProjectedVerticies, worldPos, worldNormal);

                o.vertex = mul(UNITY_MATRIX_VP, float4(estimatedPosition, 1));
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.flux = flux;
                o.distance = distance(worldPos, estimatedPosition);
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                float isVisible = 0;
                float resultingFluxValue = i.flux * _DebugFluxMultiplier;
                if (col.r != 0 || col.g != 0 || col.b != 0)
                {
                    isVisible = 1;
                }



                return float4(resultingFluxValue, i.distance, resultingFluxValue, isVisible);
            }

            ENDCG
        }
    }
}
