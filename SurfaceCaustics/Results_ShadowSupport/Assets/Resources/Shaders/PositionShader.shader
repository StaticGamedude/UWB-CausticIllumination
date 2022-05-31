/*
* WARNING: This shader should be deprecated
*/
Shader "Unlit/PositionShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "SpecularObj" = "1" }
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
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                float3 worldPos = mul(UNITY_MATRIX_M, v.vertex);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = worldPos;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                // Sample the texture to determine whether or not this fragment is trying to actually render something to the screen.
                // If the sample is completely black (i.e. r,g,b == 0), then we'll set the alpha channel to 0, otherwise we'll set it to 1.
                fixed4 col = tex2D(_MainTex, i.uv);
                float isVisible = 0;

                if (col.r != 0 || col.g != 0 && col.b != 0)
                {
                    isVisible = 1;
                }

                return float4(i.worldPos.x, i.worldPos.y, i.worldPos.z, isVisible);
            }
            ENDCG
        }
    }
}
