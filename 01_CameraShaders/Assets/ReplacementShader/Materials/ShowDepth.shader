// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/ShowDepth"
{
    Properties
    {
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "DrewShader"="Drew1" }
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
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float depth : DEPTH;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                //_ProjectionParams is a built in shader variable. _ProjectionParams.w is 1 / the camera's far plane
                o.depth = -mul(UNITY_MATRIX_MV, v.vertex).z * _ProjectionParams.w; //Depth should be scaled from 0.0 to 1.0 here
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float invert = 1 - i.depth; //The further away to fragment, the darker in color it should be
                return fixed4(invert, invert, invert, 1);
            }
            ENDCG
        }
    }
}
