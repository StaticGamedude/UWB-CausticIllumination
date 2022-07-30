Shader "Unlit/CausticSplatPositionShader_0"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "SpecularObj" = "1"}
        LOD 100
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "CommonFunctions.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 splatPos : TEXCOORD1;
            };

            //Light specific parameters
            sampler2D _ReceivingPosTexture_0;
            float4x4 _LightViewProjectionMatrix_0;
            float3 _LightWorldPosition_0;
            float3 _LightCam_Forward_0;
            int _LightIsDirectional_0;

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _ObjectRefractionIndex;

            v2f vert (appdata v)
            {
                v2f o;
                float3 worldPos = mul(UNITY_MATRIX_M, v.vertex);
                float3 worldNormal = normalize(mul(transpose(unity_WorldToObject), v.normal));
                float3 estimatedPosition = GetEstimatedSplatPosition(
                    _LightViewProjectionMatrix_0,
                    _LightWorldPosition_0,
                    _LightCam_Forward_0,
                    _LightIsDirectional_0,
                    _ObjectRefractionIndex,
                    worldPos,
                    worldNormal,
                    _ReceivingPosTexture_0);

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.splatPos = estimatedPosition;
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                return float4(i.splatPos, 1);
            }
            ENDCG
        }
    }
}
