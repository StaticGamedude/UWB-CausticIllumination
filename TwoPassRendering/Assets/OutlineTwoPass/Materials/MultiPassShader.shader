Shader "Unlit/MultiPassShader"
{
    Properties
    {
        _FirstColor("First Color", Color) = (1, 0, 0, 1)
        _SecondColor("Second Color", Color) = (0, 1, 0, 1)
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
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };

            fixed4 _FirstColor;

            v2f vert (appdata v)
            {
                v2f o;
                v.vertex -= float4(1.5, 0, 0, 0);
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return _FirstColor;
            }
            ENDCG
        }

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
            };

            fixed4 _SecondColor;

            v2f vert(appdata v)
            {
                v2f o;
                v.vertex += float4(1.5, 0, 0, 0);
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return _SecondColor;
            }
            ENDCG
        }
    }
}
