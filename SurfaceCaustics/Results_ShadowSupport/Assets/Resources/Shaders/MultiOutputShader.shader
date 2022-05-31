/*
* WARNING: This shader should be deprecated
*/
Shader "Unlit/MultiOutputShader"
{
    SubShader
    {
        Pass
        {
            ZTest Always Cull Off ZWrite Off
              Fog { Mode off }

            CGPROGRAM
            #include "UnityCG.cginc"
            #pragma vertex vert
            #pragma fragment frag

            struct v2f
            {
                float4  pos : SV_POSITION;
                float2  uv : TEXCOORD0;
            };

            struct f2a
            {
                float4 col0 : COLOR0;
                float4 col1 : COLOR1;
                float4 col2 : COLOR2;
                float4 col3 : COLOR3;
            };

            v2f vert(appdata_base v)
            {
                v2f OUT;
                OUT.pos = UnityObjectToClipPos(v.vertex);
                OUT.uv = v.texcoord.xy;
                return OUT;
            }


            f2a frag(v2f IN)
            {

                f2a OUT;
                OUT.col0 = float4(1, 0, 0, 1);
                OUT.col1 = float4(0, 1, 0, 1);
                OUT.col2 = float4(0, 0, 1, 1);
                OUT.col3 = float4(1, 1, 0, 1);
                return OUT;
            }

            ENDCG

        }
    }
}