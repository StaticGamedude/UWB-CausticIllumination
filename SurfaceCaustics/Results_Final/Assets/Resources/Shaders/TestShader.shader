Shader "Unlit/TestShader"
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

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            sampler2D _ReceivingPosTexture_0;
            float4x4 _LightViewProjectionMatrix_0;

            float3 ProjectPos(float3 worldPos)
            {
                float4 texPt = mul(_LightViewProjectionMatrix_0, float4(worldPos, 1));
                float2 tc = 0.5 * (texPt.xy / texPt.w) + float2(0.5, 0.5);
                float4 recPos = tex2Dlod(_ReceivingPosTexture_0, float4(tc, 1, 1));
                return recPos;
            }

            v2f vert (appdata v)
            {
                v2f o;
                float3 worldPos = mul(UNITY_MATRIX_M, v.vertex);
                float3 projectedPos = ProjectPos(worldPos);
                //o.vertex = UnityObjectToClipPos(v.vertex);
                o.vertex = mul(UNITY_MATRIX_VP, float4(projectedPos, 1));
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                // sample the texture
                /*fixed4 col = tex2D(_MainTex, i.uv);
                return col;*/
                return float4(1, 1, 1, 1);
            }
            ENDCG
        }
    }
}
