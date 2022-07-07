Shader "Unlit/CausticPass1DataShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "SpecularObject" = "1"}
        ZWrite Off
        Cull Off
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
                float flux : TEXCOORD2;
                float distanceVal : TEXCOORD3;
            };

            float3 RefractRay(float3 lightPosition, float3 specularVertexWorldPos, float3 specularVertexWorldNormal, float refractionIndex)
            {
                float3 vertexToLight = normalize(lightPosition - specularVertexWorldPos);
                float incidentAngle = acos(dot(vertexToLight, specularVertexWorldNormal));
                float refractionAngle = asin(sin(incidentAngle) / refractionIndex);
                float3 refractedRay = -1 * ((vertexToLight / refractionIndex) + ((cos(refractionAngle) - (cos(incidentAngle) / refractionIndex)) * specularVertexWorldNormal));
                return normalize(refractedRay);
            }

            float3 ProjectPointOntoReceiver(float3 receiverPos, float3 receiverNormal, float3 pointToProject)
            {
                float3 posToFloorVector = pointToProject - receiverPos;
                float D = dot(receiverNormal, receiverPos);
                float d = dot(receiverNormal, pointToProject);
                float3 resultingPosition = pointToProject - ((d - D) * receiverNormal);
                return resultingPosition;
            }

            float3 VertexEstimateIntersection(
                float3 specularVertexWorldPos,
                float3 refractedLightRayDirection,
                sampler2D positionTexture)
            {
                float3 barrierNormal = float3(0, 1, 0);
                float3 barrierPosition = float3(0, 0, 0);

                float3 p1 = specularVertexWorldPos + (1.0 * refractedLightRayDirection); 
                float3 p1Projected = ProjectPointOntoReceiver(barrierPosition, barrierNormal, p1);
                float newDistance = distance(specularVertexWorldPos, p1Projected);
                float3 p2 = specularVertexWorldPos + (newDistance * refractedLightRayDirection); 
                float3 p2Projected = ProjectPointOntoReceiver(barrierPosition, barrierNormal, p2);

                return p2Projected;
            }

            float GetFluxContribution(float3 lightWorldPos, float visibleSurfaceArea, float3 worldPosition, float3 worldNormal)
            {
                float3 incidentLightVector = normalize(lightWorldPos - worldPosition);

                //Because of the possibility of negative angles between the normal and light vector, we clamp the value
                return (1 / visibleSurfaceArea) * max(dot(worldNormal, incidentLightVector), 0.01);
            }

            sampler2D _MainTex;
            sampler2D _ReceiverPositions;
            float4 _MainTex_ST;

            float3 _LightPosition;
            float _RefractionIndex;
            int _VisibleSurfaceArea;
            float _FluxMultiplier;

            v2f vert (appdata v)
            {
                float3 barrierNormal = float3(0, 1, 0);
                float3 barrierPosition = float3(0, 0, 0);
                float3 vertexWorldPos = mul(UNITY_MATRIX_M, v.vertex);
                float3 worldNorm = mul(transpose(unity_WorldToObject), v.normal);
                float3 refractedRayDirection = RefractRay(_LightPosition, vertexWorldPos, worldNorm, _RefractionIndex);
                float3 resultingPosition = ProjectPointOntoReceiver(barrierPosition, barrierNormal, vertexWorldPos);
                float3 splatPos = VertexEstimateIntersection(vertexWorldPos, refractedRayDirection, _ReceiverPositions);
                float flux = GetFluxContribution(_LightPosition, _VisibleSurfaceArea, vertexWorldPos, worldNorm);
                float distanceVal = distance(vertexWorldPos, splatPos);



                v2f o;
                o.vertex = mul(UNITY_MATRIX_VP, float4(splatPos, 1)); /*UnityObjectToClipPos(v.vertex)*/;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = splatPos;
                o.flux = flux * _FluxMultiplier;
                o.distanceVal = distanceVal;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                //return fixed4(i.worldPos, 1);
                return fixed4(i.flux, i.distanceVal, 1, 1);
            }
            ENDCG
        }
    }
}
