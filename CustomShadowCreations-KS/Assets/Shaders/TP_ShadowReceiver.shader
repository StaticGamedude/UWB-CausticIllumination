Shader "Unlit/TP_ShadowReceiver"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
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
                float4 shadowCoords: TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4x4 _ShadowMatrix; //Set externally by shadow caster script
            sampler2D _ShadowTex; //Set externally by shadow caster script
            float _ShadowBias; //Set externally by shadow caster script

            v2f vert (appdata v)
            {
                v2f o;
                float3 worldPos = mul(UNITY_MATRIX_M, v.vertex);

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.shadowCoords = mul(_ShadowMatrix, float4(worldPos, 1.0));
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float lightDepth = 1 - tex2D(_ShadowTex, i.shadowCoords.xy / i.shadowCoords.w).r;
                float shadow = (i.shadowCoords.z - _ShadowBias) < lightDepth ? 1.0 : 0.5;
                fixed4 col = tex2D(_MainTex, i.uv);
                col = col * shadow;
                return col;
            }
            ENDCG
        }
    }
}
