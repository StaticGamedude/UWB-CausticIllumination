Shader "Unlit/CausticDistanceShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "SpecularObj" = "1"  }
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
                float3 worldPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _CausticMapTexture;

            v2f vert (appdata v)
            {
                v2f o;
                float3 worldPos = mul(UNITY_MATRIX_M, v.vertex);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = worldPos;
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float3 splatPos = tex2D(_CausticMapTexture, i.uv).xyz;
                float d = distance(splatPos, i.worldPos);
                return float4(d, d, d, 1);

                //return float4(10, 10, 10, 1);
            }
            ENDCG
        }
    }
}
