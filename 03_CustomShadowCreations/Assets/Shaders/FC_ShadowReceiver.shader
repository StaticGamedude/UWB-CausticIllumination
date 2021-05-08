Shader "Unlit/FC_ShadowReceiver"
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
            float4x4 _FC_ShadowMatrix; //Set externally by shadow caster script
            sampler2D _FC_ShadowTex; //Set externally by shadow caster script
            float _FC_ShadowBias; //Set externally by shadow caster script

            v2f vert (appdata v)
            {
                v2f o;
                float3 worldPos = mul(UNITY_MATRIX_M, v.vertex);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.shadowCoords = mul(_FC_ShadowMatrix, float4(worldPos, 1.0));
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                //float lightDepth = 1 - tex2D(_FC_ShadowTex, i.shadowCoords.xy / i.shadowCoords.w).r; //Relying on color to get depth information. Only once channel needed
                //float shadow = (i.shadowCoords.z - _FC_ShadowBias) < lightDepth ? 1.0 : 0.5;
                //fixed4 col = tex2D(_MainTex, i.uv);
                //col = col * shadow;
                //return col;

                /*float d = i.shadowCoords.z;
                return fixed4(d, d, d, 1);*/

                //Working!
                float lightDepth = tex2D(_FC_ShadowTex, i.shadowCoords.xy / i.shadowCoords.w).r; //Relying on color to get depth information. Only once channel needed
                return fixed4(lightDepth, lightDepth, lightDepth, 1);

                //float lightDepth = 1 - tex2D(_FC_ShadowTex, i.shadowCoords.xy / i.shadowCoords.w).r; //Relying on color to get depth information. Only once channel needed
                //float shadow = lightDepth < 0.5 ? 0.5 : 1;
                //return fixed4(shadow, shadow, shadow, 1);
            }
            ENDCG
        }
    }
}
