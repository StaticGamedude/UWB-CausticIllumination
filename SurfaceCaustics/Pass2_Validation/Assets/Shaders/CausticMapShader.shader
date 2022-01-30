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
                //float3 splatPos : TEXCOORD3;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _ReceivingPosTexture;
            float4x4 _LightViewProjectionMatrix;
            int _EstimateIntersectionLevel;

            // This method is working, but it has to be run in the fragment shader so we can run tex2D for sampling the texture
            float3 EstimateIntersection(float3 specularVertexWorldPos, float3 specularVertexWorldNorm, sampler2D positionTexture)
            {
                float3 refractedLightRayDirection;
                float3 p1;
                float4 recPos;
                float4 texPt;
                float2 tc;

                if (_EstimateIntersectionLevel == 0)
                {
                    // Simply return 1 off the vertex world position in the direction of the vertex normal
                    return specularVertexWorldPos + (1.0 * specularVertexWorldNorm);
                }
                
                // We flip the normal direction which will act ours our refracted ray direction for now
                refractedLightRayDirection = -1 * specularVertexWorldNorm;
                p1 = specularVertexWorldPos + (1.0 * refractedLightRayDirection);
                if (_EstimateIntersectionLevel == 1)
                {
                    return p1;
                }

                //P1 projection into light image space
                texPt = mul(_LightViewProjectionMatrix, float4(p1, 1));
                tc = 0.5 * texPt.xy / texPt.w + float2(0.5, 0.5);
                tc.y = 1 - tc.y;
                recPos = tex2D(_ReceivingPosTexture, tc);
                if (_EstimateIntersectionLevel == 2)
                {
                    return float3(recPos.x, recPos.y, recPos.z);
                }

                if (_EstimateIntersectionLevel == 3)
                {
                    float3 p2 = specularVertexWorldPos + (distance(specularVertexWorldPos, recPos.xzy) * refractedLightRayDirection);
                    texPt = mul(_LightViewProjectionMatrix, float4(p2, 1));
                    tc = 0.5 * texPt.xy / texPt.w + float2(0.5, 0.5);
                    tc.y = 1 - tc.y;
                    recPos = tex2D(_ReceivingPosTexture, tc);
                    return float3(recPos.x, recPos.y, recPos.z);
                }

                // The logic below was proposed by Dr. Sung as another way to help verify what we're doing
                /*texPt = mul(float4(specularVertexWorldPos, 1), _LightViewProjectionMatrix);*/
                texPt = mul(_LightViewProjectionMatrix, float4(specularVertexWorldPos, 1));
                tc = 0.5 * texPt.xy / texPt.w + float2(0.5, 0.5);
                tc.y = 1 - tc.y;
                recPos = tex2D(_ReceivingPosTexture, tc);
                return float3(recPos.x, recPos.y, recPos.z);
            }

            v2f vert (appdata v)
            {
                v2f o;
                float3 specularVertexWorldPos = mul(UNITY_MATRIX_M, v.vertex);
                float3 specularVertexWorldNormal = mul(transpose(unity_WorldToObject), v.normal); //mul(UNITY_MATRIX_M, v.normal);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                o.specularVertexWorldPos = specularVertexWorldPos;
                o.worldRefractedRayDirection = specularVertexWorldNormal; //TODO: We should refract the normal here. For now, we'll simply point it in the opposite direction
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {

                float3 splatPosition = EstimateIntersection(i.specularVertexWorldPos, i.worldRefractedRayDirection, _ReceivingPosTexture);
                return float4(splatPosition.xyz, 1);
            }
            ENDCG
        }
    }
}
