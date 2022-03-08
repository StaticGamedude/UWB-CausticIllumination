Shader "Unlit/PlaneShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Specular"="1" }
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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            bool IsAllWhite(float4 color)
            {
                return color.r == 1 && color.g == 1 && color.b == 1;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed2 uv = round(i.uv * 256) / 256; //Assume texture is 256x256
                half s = 1 / 256;

                float4 cl = tex2D(_MainTex, uv + fixed2(-s, 0));    // Centre Left
                float4 tc = tex2D(_MainTex, uv + fixed2(-0, -s));    // Top Centre
                float4 cc = tex2D(_MainTex, uv + fixed2(0, 0));    // Centre Centre
                float4 bc = tex2D(_MainTex, uv + fixed2(0, +s));    // Bottom Centre
                float4 cr = tex2D(_MainTex, uv + fixed2(+s, 0));    // Centre Right

                if (IsAllWhite(cl) && IsAllWhite(tc) && IsAllWhite(cr) && IsAllWhite(bc))
                {
                    return fixed4(0, 1, 0, 1);
                }
                else if (IsAllWhite(cl) && IsAllWhite(cr))
                {
                    return fixed4(0, 0, 1, 1);
                }
                else if (IsAllWhite(tc) && IsAllWhite(bc))
                {
                    return fixed4(1, 0, 0, 1);
                }
                else
                {
                    return fixed4(1, 1, 1, 1);
                }
            }
            ENDCG
        }
    }
}
