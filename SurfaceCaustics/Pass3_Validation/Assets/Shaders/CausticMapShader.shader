Shader "Unlit/CausticMapShader"
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

                float3 specularVertexWorldPos : TEXCOORD1;
                float3 worldRefractedRayDirection : TEXCOORD2;
                float flux : TEXCOORD3;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _ReceivingPosTexture;
            float4x4 _LightViewProjectionMatrix;
            float3 _LightWorldPosition;
            float _RefractiveIndex;
            int _NumProjectedVerticies;

            RWTexture2D<float> TotalFluxBuffer : register(u1);

            float GetFluxContribution(float visibleSurfaceArea, float3 worldPosition, float3 worldNormal)
            {
                float3 incidentLightVector = normalize(_LightWorldPosition - worldPosition);
                return (1 / visibleSurfaceArea) * dot(worldNormal, incidentLightVector);
            }

            float3 EstimateIntersection(float3 specularVertexWorldPos, float3 refractedLightRayDirection, sampler2D positionTexture)
            {
                float3 p1 = specularVertexWorldPos + (1.0 * refractedLightRayDirection);;
                float4 texPt = mul(_LightViewProjectionMatrix, float4(p1, 1));
                float2 tc = 0.5 * texPt.xy / texPt.w + float2(0.5, 0.5);
                
                float4 recPos = tex2D(_ReceivingPosTexture, tc);
                float3 p2 = specularVertexWorldPos + (distance(specularVertexWorldPos, recPos.xzy) * refractedLightRayDirection);
                texPt = mul(_LightViewProjectionMatrix, float4(p2, 1));
                tc = 0.5 * texPt.xy / texPt.w + float2(0.5, 0.5);
                return tex2D(_ReceivingPosTexture, tc);
            }

            float3 RefractRay(float3 specularVertexWorldPos, float3 specularVertexWorldNormal)
            {
                float3 lightToVertex = specularVertexWorldPos - _LightWorldPosition;
                float3 normalizedLightToVertexDirection = normalize(specularVertexWorldPos - _LightWorldPosition);
                float incidentAngle = dot(normalizedLightToVertexDirection, specularVertexWorldNormal);
                float refractionAngle = asin(sin(incidentAngle) / _RefractiveIndex);
                float3 refractedRay = -1 * (lightToVertex / _RefractiveIndex) + (cos(refractionAngle) - (cos(incidentAngle / _RefractiveIndex))) * specularVertexWorldNormal;
                return refractedRay;
            }

            void UpdateFluxTotal(float3 splatPosition, float flux)
            {
                /*float2 tc = mul(_LightViewProjectionMatrix, float4(splatPosition, 1));
                InterlockedAdd(TotalFluxBuffer[tc], flux);*/
            }

            v2f vert (appdata v)
            {
                v2f o;
                float3 specularVertexWorldPos = mul(UNITY_MATRIX_M, v.vertex);
                float3 specularVertexWorldNormal = mul(transpose(unity_WorldToObject), v.normal);
                float3 flux = GetFluxContribution(_NumProjectedVerticies, specularVertexWorldPos, specularVertexWorldNormal);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                o.specularVertexWorldPos = specularVertexWorldPos;
                o.worldRefractedRayDirection = RefractRay(specularVertexWorldPos, specularVertexWorldNormal);
                o.flux = flux;
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float3 splatPosition = EstimateIntersection(i.specularVertexWorldPos, i.worldRefractedRayDirection, _ReceivingPosTexture);
                UpdateFluxTotal(splatPosition, i.flux);
                return float4(splatPosition.xyz, 1);
            }
            ENDCG
        }
    }
}
