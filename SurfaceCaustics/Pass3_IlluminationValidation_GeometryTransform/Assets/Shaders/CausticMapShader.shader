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
                float3 vertexCreatedPos : TEXCOORD3;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _ReceivingPosTexture;
            float4x4 _LightViewProjectionMatrix;
            float3 _LightWorldPosition;
            float _RefractiveIndex;

            float3 EstimateIntersection(float3 specularVertexWorldPos, float3 refractedLightRayDirection, sampler2D positionTexture)
            {
                float3 p1 = specularVertexWorldPos + (1.0 * refractedLightRayDirection);;
                float4 texPt = mul(_LightViewProjectionMatrix, float4(p1, 1));
                float2 tc = 0.5 * texPt.xy / texPt.w + float2(0.5, 0.5);
                
                float4 recPos = tex2D(_ReceivingPosTexture, tc); //projected p1 position
                float3 p2 = specularVertexWorldPos + (distance(specularVertexWorldPos, recPos.xzy) * refractedLightRayDirection);
                texPt = mul(_LightViewProjectionMatrix, float4(p2, 1));
                tc = 0.5 * texPt.xy / texPt.w + float2(0.5, 0.5);

                return tex2D(_ReceivingPosTexture, tc); //projected p2 position
            }

            float3 VertexEstimateIntersection(float3 specularVertexWorldPos, float3 refractedLightRayDirection, sampler2D positionTexture)
            {
                float3 p1 = specularVertexWorldPos + (1.0 * refractedLightRayDirection);;
                float4 texPt = mul(_LightViewProjectionMatrix, float4(p1, 1));
                float2 tc = 0.5 * texPt.xy / texPt.w + float2(0.5, 0.5);

                float4 recPos = tex2Dlod(_ReceivingPosTexture, float4(tc, 1, 1)); //projected p1 position
                float3 p2 = specularVertexWorldPos + (distance(specularVertexWorldPos, recPos.xzy) * refractedLightRayDirection);
                texPt = mul(_LightViewProjectionMatrix, float4(p2, 1));
                tc = 0.5 * texPt.xy / texPt.w + float2(0.5, 0.5);

                return tex2Dlod(_ReceivingPosTexture, float4(tc, 1, 1)); //projected p2 position
            }

            float3 RefractRay(float3 specularVertexWorldPos, float3 specularVertexWorldNormal)
            {
                float3 vertexToLight = normalize(_LightWorldPosition - specularVertexWorldPos);
                float incidentAngle = dot(vertexToLight, specularVertexWorldNormal);
                float refractionAngle = asin(sin(incidentAngle) / _RefractiveIndex);
                float3 refractedRay = -1 * ((vertexToLight / _RefractiveIndex) + ((cos(asin(sin(incidentAngle) / _RefractiveIndex)) - (cos(incidentAngle) / _RefractiveIndex)) * specularVertexWorldNormal));
                return normalize(refractedRay);
            }

            v2f vert (appdata v)
            {
                v2f o;
                float3 worldPos = mul(UNITY_MATRIX_M, v.vertex);
                float3 worldNormal = mul(transpose(unity_WorldToObject), v.normal);
                float3 refractedDirection = RefractRay(worldPos, worldNormal);
                float3 estimatedPosition = VertexEstimateIntersection(worldPos, refractedDirection, _ReceivingPosTexture);

                o.vertex = mul(UNITY_MATRIX_VP, float4(estimatedPosition, 1));
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.vertexCreatedPos = estimatedPosition;

                return o;

                /*v2f o;
                float3 specularVertexWorldPos = mul(UNITY_MATRIX_M, v.vertex);
                float3 specularVertexWorldNormal = normalize(mul(transpose(unity_WorldToObject), v.normal));
                float3 estimatedPosition = VertexEstimateIntersection(worldPos, refractedDirection, _ReceivingPosTexture);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                o.specularVertexWorldPos = specularVertexWorldPos;
                o.worldRefractedRayDirection = RefractRay(specularVertexWorldPos, specularVertexWorldNormal);
                o.vertexCreatedPos = VertexEstimateIntersection(specularVertexWorldPos, o.worldRefractedRayDirection, _ReceivingPosTexture);
                return o;*/
            }

            float4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                float isVisible = 0;

                if (col.r != 0 || col.g != 0 && col.b != 0)
                {
                    isVisible = 1;
                }

                //float3 splatPosition = EstimateIntersection(i.specularVertexWorldPos, i.worldRefractedRayDirection, _ReceivingPosTexture);                
                float3 splatPosition = i.vertexCreatedPos;
                return float4(splatPosition.xyz, isVisible);
            }
            ENDCG
        }
    }
}
