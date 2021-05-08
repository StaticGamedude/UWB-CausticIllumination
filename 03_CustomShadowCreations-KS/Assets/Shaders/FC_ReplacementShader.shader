Shader "Unlit/FC_ReplacementShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "FC_ShadowProducer" = "1"}
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
                float4 vertex : SV_POSITION;
                float depth : DEPTH;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.depth = -mul(UNITY_MATRIX_MV, v.vertex).z * _ProjectionParams.w;  // View space z the .w parameter is 1/farClippingPlane
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // sample the texture
                // float invert = 1.0 - i.depth;
                float invert = i.depth; // 0-close, 1-far;
                return fixed4(invert, invert, invert, 1);
            }
            ENDCG
        }
    }
}
