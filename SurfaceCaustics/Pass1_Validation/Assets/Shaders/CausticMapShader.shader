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

            // This method is working, but it has to be run in the fragment shader so we can run tex2D for sampling the texture
            float3 EstimateIntersection(float3 specularVertexWorldPos, float3 refractedLightRayDirection, sampler2D positionTexture)
            {
                float3 p1 = specularVertexWorldPos + (1.0 * refractedLightRayDirection);
                /*float4 texPt = mul(float4(p1, 1), _LightViewProjectionMatrix);
                float2 tc = 0.5 * texPt.xy / texPt.w + float2(0.5, 0.5);
                tc.y = 1 - tc.y;
                float4 recPos = tex2D(_ReceivingPosTexture, tc);*/

                /*float3 p2 = specularVertexWorldPos + (distance(specularVertexWorldPos, recPos.xzy) * refractedLightRayDirection);
                texPt = mul(float4(p2, 1), _LightViewProjectionMatrix);
                tc = 0.5 * texPt.xy / texPt.w + float2(0.5, 0.5);
                tc.y = 1 - tc.y;
                recPos = tex2D(_ReceivingPosTexture, tc);*/

                //return float3(recPos.x, recPos.y, recPos.z);
                return p1;
            }

            // Implementation for estimating the intersection entirely in the vertex shader. Doesn't seem that tex2Dlod work like we expected
            // TODO: Investigate why exactly this method doesn't work
            //float3 EstimateIntersection_2(float3 specularVertexWorldPos, float3 refractedLightRayDirection, sampler2D positionTexture)
            //{
            //    float3 p1 = specularVertexWorldPos + (1.0 * refractedLightRayDirection);
            //    float4 texPt = mul(float4(p1, 1), _LightViewProjectionMatrix);
            //    float2 tc = 0.5 * texPt.xy / texPt.w + float2(0.5, 0.5);
            //    tc.y = 1 - tc.y;
            //    float4 recPos = tex2Dlod(_MainTex, float4(tc, 0, 0));
            //    //float4 recPos = tex2D(_ReceivingPosTexture, tc);

            //    float3 p2 = specularVertexWorldPos + (distance(specularVertexWorldPos, recPos.xzy) * refractedLightRayDirection);
            //    texPt = mul(float4(p2, 1), _LightViewProjectionMatrix);
            //    tc = 0.5 * texPt.xy / texPt.w + float2(0.5, 0.5);
            //    tc.y = 1 - tc.y;
            //    recPos = tex2Dlod(_MainTex, float4(tc, 0, 0));
            //    //recPos = tex2D(_ReceivingPosTexture, tc);

            //    return float3(recPos.x, recPos.y, recPos.z);
            //    return p1;
            //}

            v2f vert (appdata v)
            {
                v2f o;
                float3 specularVertexWorldPos = mul(UNITY_MATRIX_M, v.vertex);
                float3 specularVertexWorldNormal = mul(UNITY_MATRIX_M, v.normal);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                o.specularVertexWorldPos = specularVertexWorldPos;
                o.worldRefractedRayDirection = specularVertexWorldNormal; //TODO: We should refract the normal here. For now, we'll simply point it in the opposite direction
                //o.splatPos = EstimateIntersection_2(specularVertexWorldPos, specularVertexWorldNormal * -1, _ReceivingPosTexture);
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
