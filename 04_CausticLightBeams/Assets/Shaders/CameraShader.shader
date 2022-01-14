Shader "Unlit/CameraShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "DrewShader"="1" }
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
                float4 lightPosCoords: TEXCOORD1;
                float4 lightNormCoords: TEXCOORD2;
                float3 worldPos : TEXCOORD3;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            sampler2D _SpecularPosTexture;
            sampler2D _SpecularNormalTexture;
            float4x4 _LightMatrix;
            float4x4 _LightCamMatrix;
            float _LightCam_Far;

            v2f vert (appdata v)
            {
                v2f o;
                float3 worldPos = mul(UNITY_MATRIX_M, v.vertex);

                o.worldPos = worldPos;

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = fixed4(0, 1, 0, 1);
                return col;
            }
            ENDCG
        }
    }
}
