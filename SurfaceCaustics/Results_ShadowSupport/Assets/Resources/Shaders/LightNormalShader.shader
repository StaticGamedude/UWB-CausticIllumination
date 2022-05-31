/*
* WARNING: This shader should be deprecated
*/
Shader "Unlit/LightNormalShader"
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
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 worldNormal : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);

                // To convert the normal to world space, get a point along the normal from the vertex.
                // then, move both points into world space. Then take the difference between the points
                // to get the world normal
                /*float3 startPoint = v.vertex.xyz;
                float3 endPoint = startPoint + (2 * v.normal);
                float3 startPointWorldPos = mul(UNITY_MATRIX_M, startPoint);
                float3 endPointWorldPos = mul(UNITY_MATRIX_M, endPoint);
                float3 worldNormal = endPointWorldPos - startPointWorldPos;*/
                //o.worldNormal = worldNormal;
                o.worldNormal = mul(transpose(unity_WorldToObject), v.normal); //mul(UNITY_MATRIX_M, v.normal);

                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                return float4(i.worldNormal.x, i.worldNormal.y, i.worldNormal.z, 1);
            }
            ENDCG
        }
    }
}
