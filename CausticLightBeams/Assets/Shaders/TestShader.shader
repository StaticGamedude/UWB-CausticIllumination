Shader "Unlit/TestShader"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
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
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert(appdata v)
            {
                v2f o;
                float3 worldPos = mul(UNITY_MATRIX_M, v.vertex);

                worldPos.x = (worldPos.x + 5) / 10;
                worldPos.y = (worldPos.y + 5) / 10;
                worldPos.z = (worldPos.z + 5) / 10;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = worldPos;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return fixed4(i.worldPos.x, i.worldPos.y, i.worldPos.z, 1);
                //return fixed4(0, 0, 1, 1);
            }
            ENDCG
        }
    }
}