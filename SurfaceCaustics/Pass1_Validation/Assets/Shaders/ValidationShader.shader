// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Unlit/ValidationShader"
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
            };

            sampler2D _SpecularPosTexture;
            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                
                v2f o;
                /*float4 testVertex = float4(v.vertex.x, v.vertex.y + 2, v.vertex.z, v.vertex.w);
                o.vertex = UnityObjectToClipPos(testVertex);*/


                float4 worldPosFromTexture = tex2Dlod(_SpecularPosTexture, float4(v.uv.xy, 0, 0));
                float4 texturePosToLocal = mul(worldPosFromTexture, unity_WorldToObject);
                o.vertex = UnityObjectToClipPos(texturePosToLocal);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = fixed4(1, 0, 0, 1);
                return col;
            }
            ENDCG
        }
    }
}
