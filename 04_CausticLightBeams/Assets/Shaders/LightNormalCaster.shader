Shader "Unlit/LightNormalCaster"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "SpecularObj"="1" }
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
                half3 normal : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert(float4 vertex : POSITION, float3 normal : NORMAL)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(vertex);
                
                // To convert the normal to world space, get a point along the normal from the vertex.
                // then, move both points into world space. Then take the difference between the points
                // to get the world normal
                float3 startPoint = vertex.xyz;
                float3 endPoint = startPoint + (2 * normal);
                float3 startPointWorldPos = mul(UNITY_MATRIX_M, startPoint);
                float3 endPointWorldPos = mul(UNITY_MATRIX_M, endPoint);
                float3 worldNormal = endPointWorldPos - startPointWorldPos;
                o.normal = worldNormal;

                return o;
            }

            float3 frag(v2f i) : SV_Target
            {
                //Simply return the normal for now. Stored as model space. We might need to convert this to world space
                return i.normal;
            }
            ENDCG
        }
    }
}
