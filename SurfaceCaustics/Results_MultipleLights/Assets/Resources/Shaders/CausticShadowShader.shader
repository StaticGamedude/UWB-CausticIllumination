Shader "Unlit/CausticShadowShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "SpecularObj" = "1"}
        LOD 100

        Pass
        {
            CGPROGRAM
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
            };

            //Light specific parameters
            sampler2D _ReceivingPosTexture_0;
            float4x4 _LightViewProjectionMatrix_0;
            float3 _LightWorldPosition_0;

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _ObjectRefractionIndex;
            int _NumProjectedVerticies;

            float3 ProjectWorldPosToReceiver(float4x4 lightViewProjeciontMatrix, float3 specularVertexWorldPos, sampler2D positionTexture)
            {
                float4 texPt = mul(lightViewProjeciontMatrix, float4(specularVertexWorldPos, 1));
                float2 tc = 0.5 * (texPt.xy / texPt.w) + float2(0.5, 0.5);
                return tex2Dlod(positionTexture, float4(tc, 1, 1)).xyz;
            }

            v2f vert (appdata v)
            {
                v2f o;
                float3 worldPos = mul(UNITY_MATRIX_M, v.vertex);
                float3 worldNormal = normalize(mul(transpose(unity_WorldToObject), v.normal));
                float3 projectedPosition = ProjectWorldPosToReceiver(_LightViewProjectionMatrix_0, worldPos, _ReceivingPosTexture_0);

                //o.vertex = UnityObjectToClipPos(v.vertex);
                o.vertex = mul(UNITY_MATRIX_VP, float4(projectedPosition, 1));
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return fixed4(1, 1, 1, 1);
            }
            ENDCG
        }
    }
}
