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

                float3 p1 = specularVertexWorldPos + (1.0 * refractedLightRayDirection); //P1 - 1 unit along the refracted ray direction from the specular vertex position
                float4 texPt = float4(ProjectPointOntoReceiver(barrierPosition, barrierNormal, p1), 1);
                float2 tc = 0.5 * (texPt.xy / texPt.w) + float2(0.5, 0.5);
                float4 recPos = tex2Dlod(positionTexture, float4(tc, 1, 1)); //Projected P1 position into the light's space;
                float newDistance = distance(specularVertexWorldPos, recPos.xyz);
                float3 p2 = specularVertexWorldPos + (newDistance * refractedLightRayDirection); //P2 - D units Point along the refracted ray direction, where D is the distance from the vertex to the p1 projected position

                texPt = float4(ProjectPointOntoReceiver(barrierPosition, barrierNormal, p2), 1);
                tc = 0.5 * (texPt.xy / texPt.w) + float2(0.5, 0.5);
                return tex2Dlod(positionTexture, float4(tc, 1, 1)); //Project P2 position into the light's space
            }

            float3 VertexEstimateIntersection_2(
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

            sampler2D _MainTex;
            sampler2D _ReceiverPositions;
            float4 _MainTex_ST;

            float3 _LightPosition;
            float _RefractionIndex;
            int _VisibleSurfaceArea;

            v2f vert (appdata v)
            {
                float3 barrierNormal = float3(0, 1, 0);
                float3 barrierPosition = float3(0, 0, 0);
                float3 vertexWorldPos = mul(UNITY_MATRIX_M, v.vertex);
                float3 worldNorm = mul(transpose(unity_WorldToObject), v.normal);
                float3 refractedRayDirection = RefractRay(_LightPosition, vertexWorldPos, worldNorm, _RefractionIndex);
                /*float3 posToFloorVector = vertexWorldPos - barrierPosition;
                float D = dot(barrierNormal, barrierPosition);
                float d = dot(barrierNormal, vertexWorldPos);*/
                //float3 resultingPosition = vertexWorldPos - ((d - D) * barrierNormal);
                float3 resultingPosition = ProjectPointOntoReceiver(barrierPosition, barrierNormal, vertexWorldPos);
                float3 splatPos = VertexEstimateIntersection_2(vertexWorldPos, refractedRayDirection, _ReceiverPositions);

                v2f o;
                o.vertex = mul(UNITY_MATRIX_VP, float4(splatPos, 1)); /*UnityObjectToClipPos(v.vertex)*/;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = splatPos;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return fixed4(i.worldPos, 1);
            }
            ENDCG
        }
    }
}
