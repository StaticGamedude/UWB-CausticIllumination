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
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };

            struct f2a
            {
                float4 col0;
                float4 col1;
                float4 col2;
                float4 col3;
            };

            v2f vert (appdata v)
            {
                v2f o;
                float3 worldPos = mul(UNITY_MATRIX_M, v.vertex);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = worldPos;
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                return float4(i.worldPos.x, i.worldPos.y, i.worldPos.z, 1);
            }
            ENDCG

            /////////////////////////Multi-output test
            //CGPROGRAM
            //#include "UnityCG.cginc"
            //#pragma vertex vert
            //#pragma fragment frag

            //struct appdata
            //{
            //    float4 vertex : POSITION;
            //    float3 normal : TEXCOORD0;
            //};

            ///*struct v2f
            //{
            //    float4  pos : SV_POSITION;
            //    float2  uv : TEXCOORD0;
            //};*/

            //struct v2f
            //{
            //    float4 vertex : SV_POSITION;
            //    float3 worldPos : TEXCOORD0;
            //    float3 normal : TEXCOORD1;
            //};

            //struct f2a
            //{
            //    float4 col0 : COLOR0;
            //    float4 col1 : COLOR1;
            //    float4 col2 : COLOR2;
            //    float4 col3 : COLOR3;
            //};

            //v2f vert (appdata v)
            //{
            //    v2f o;
            //    float3 worldPos = mul(UNITY_MATRIX_M, v.vertex);
            //    o.vertex = UnityObjectToClipPos(v.vertex);
            //    o.worldPos = worldPos;

            //    ////

            //    // To convert the normal to world space, get a point along the normal from the vertex.
            //    // then, move both points into world space. Then take the difference between the points
            //    // to get the world normal
            //    float3 startPoint = v.vertex.xyz;
            //    float3 endPoint = startPoint + (2 * v.normal);
            //    float3 startPointWorldPos = mul(UNITY_MATRIX_M, startPoint);
            //    float3 endPointWorldPos = mul(UNITY_MATRIX_M, endPoint);
            //    float3 worldNormal = endPointWorldPos - startPointWorldPos;
            //    o.normal = worldNormal;

            //    return o;
            //}


            //f2a frag(v2f i)
            //{

            //    f2a OUT;
            //    //OUT.col0 = float4(func1, func2, ...)
            //    OUT.col0 = float4(i.worldPos.x, i.worldPos.y, i.worldPos.z, 1);
            //    OUT.col1 = float4(i.normal.x, i.normal.y, i.normal.z, 1);
            //    OUT.col2 = float4(0, 0, 1, 1);
            //    OUT.col3 = float4(1, 1, 0, 1);
            //    //OUT.col3 = float4(94, 11, 97, 1);
            //    //OUT.col3 = float4(0.5, 0.5, 0.5, 0.5);

            //    return OUT;
            //}

            //ENDCG
        }
    }
}
