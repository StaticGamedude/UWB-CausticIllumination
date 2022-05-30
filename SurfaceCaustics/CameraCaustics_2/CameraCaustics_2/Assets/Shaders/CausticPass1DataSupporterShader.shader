Shader "Unlit/CausticPass1DataSupporterShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "SpecularObject"="1"}
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
                float3 direction : TEXCOORD1;
            };

            float3 RefractRay(float3 lightPosition, float3 specularVertexWorldPos, float3 specularVertexWorldNormal, float refractionIndex)
            {
                float3 vertexToLight = normalize(lightPosition - specularVertexWorldPos);
                float incidentAngle = acos(dot(vertexToLight, specularVertexWorldNormal));
                float refractionAngle = asin(sin(incidentAngle) / refractionIndex);
                float3 refractedRay = -1 * ((vertexToLight / refractionIndex) + ((cos(refractionAngle) - (cos(incidentAngle) / refractionIndex)) * specularVertexWorldNormal));
                return normalize(refractedRay);
            }

            float GetFluxContribution(float3 lightWorldPos, float visibleSurfaceArea, float3 worldPosition, float3 worldNormal)
            {
                float3 incidentLightVector = normalize(lightWorldPos - worldPosition);

                //Because of the possibility of negative angles between the normal and light vector, we clamp the value
                return (1 / visibleSurfaceArea) * max(dot(worldNormal, incidentLightVector), 0.01);
            }

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float3 _LightPosition;
            float _RefractionIndex;
            int _VisibleSurfaceArea;

            v2f vert (appdata v)
            {
                v2f o;
                float3 worldPos = mul(UNITY_MATRIX_M, v.vertex);
                float3 worldNorm = mul(transpose(unity_WorldToObject), v.normal);
                float3 refractedRayDirection = RefractRay(_LightPosition, worldPos, worldNorm, _RefractionIndex);
                float flux = GetFluxContribution(_LightPosition, _VisibleSurfaceArea, worldPos, worldNorm);

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.direction = refractedRayDirection; //worldNorm;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return fixed4(i.direction, 1);
            }
            ENDCG
        }
    }
}
