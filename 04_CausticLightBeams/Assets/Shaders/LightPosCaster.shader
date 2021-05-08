Shader "Unlit/LightPosCaster"
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

            v2f vert(float4 vertex : POSITION, float3 normal : NORMAL)
            {
                v2f o;
                float3 worldPos = mul(UNITY_MATRIX_M, vertex);

                o.vertex = UnityObjectToClipPos(vertex);
                o.worldPos = worldPos;
                return o;
            }

            // Ideally, if we can, we could simply return a float3 for the position information
            /*float3 frag(v2f i) : SV_Target
            {
                return i.worldPos;
            }*/

            // Since, it doesn't look like there's a way to specifically a float3 on unity's end,
            // I'm resorting back to return the value as fixed4.
            fixed4 frag(v2f i) : SV_Target
            {
                //return fixed4(i.worldPos.x, i.worldPos.y, i.worldPos.z, 1);
                return fixed4(0, 0, 1, 1);
            }
            ENDCG
        }
    }
}
