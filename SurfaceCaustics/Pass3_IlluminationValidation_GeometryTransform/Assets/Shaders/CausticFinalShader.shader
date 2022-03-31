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
            // make fog work
            #pragma multi_compile_fog

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
                float causticIntensity : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float3 _LightWorldPosition;
            float _LightIntensity;
            int _NumProjectedVerticies;
            float _AbsorptionCoefficient;
            float _RefractionDistance;

            float GetFluxContribution(float visibleSurfaceArea, float3 worldPosition, float3 worldNormal)
            {
                float3 incidentLightVector = normalize(worldPosition - _LightWorldPosition);
                return (1 / visibleSurfaceArea) * dot(worldNormal, incidentLightVector);
            }

            float ComputeCausticIntensity(float lightIntesnity, float absorptionCoefficient, float distanceThroughSpecularObject)
            {
                float e = 2.718282; //Not sure how to reference the internal constant from Unity
                return lightIntesnity * pow(e, (absorptionCoefficient * 1));
                //return lightIntesnity * exp(-absorptionCoefficient * 20);
            }

            v2f vert (appdata v)
            {
                v2f o;
                float3 worldPos = mul(UNITY_MATRIX_M, v.vertex);
                float3 worldNormal = mul(transpose(unity_WorldToObject), v.normal);
                float fluxContribution = GetFluxContribution(_NumProjectedVerticies, worldPos, worldNormal);
                float causticIntensity = ComputeCausticIntensity(_LightIntensity, fluxContribution, _RefractionDistance);

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.causticIntensity = causticIntensity;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                float isVisible = 0;

                if (col.r != 0 || col.g != 0 && col.b != 0)
                {
                    isVisible = 1;
                }

                return float4(i.causticIntensity, i.causticIntensity, i.causticIntensity, isVisible);
            }
            ENDCG
        }
    }
}
