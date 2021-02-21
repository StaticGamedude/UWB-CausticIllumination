Shader "Unlit/ShadowShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (0, 0, 1, 1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
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
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _PlaneNormal;
            fixed4 _Color;

            v2f vert (appdata v)
            {
                v2f o;

                //In forward base light mode _WorldSpaceLightPos0 is always direction light
                float4 worldLightDirection = -normalize(_WorldSpaceLightPos0);

                //Calculate vertex offset
                float planeNormalDotWorldVertex = dot(_PlaneNormal, mul(unity_ObjectToWorld, v.vertex));
                float planeNormalDotLightDir = dot(_PlaneNormal, worldLightDirection);
                float3 worldVertexToPlaneVector = worldLightDirection * (planeNormalDotWorldVertex / (-planeNormalDotLightDir));
                
                //Add vertex offset in local coordinates before applying final transformation
                o.vertex = UnityObjectToClipPos(v.vertex + mul(unity_WorldToObject, worldVertexToPlaneVector));
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = _Color;
                return col;
            }
            ENDCG
        }
    }
}
