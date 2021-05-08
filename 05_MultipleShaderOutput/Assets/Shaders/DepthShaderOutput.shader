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
                float depth : DEPTH;
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

                OUT.depth = 5.5;
                //OUT.pos0 = float4(5.5, 5.5, 5.5, 5.5);

                return OUT;
            }

            ENDCG

        }
    }
}