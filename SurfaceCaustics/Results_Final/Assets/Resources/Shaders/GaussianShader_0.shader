/*
* WARNING: This shader should be deprecated
*/
Shader "Unlit/GaussianShader_0"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "SpecularObj" = "1"}
        LOD 100

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
                float3 worldPos : TEXCOORD1;
            };

            //Light specific parameters
            sampler2D _ReceivingPosTexture_0;
            float4x4 _LightViewProjectionMatrix_0;
            float3 _LightWorldPosition_0;
            float3 _LightCam_Forward_0;
            int _LightIsDirectional_0;

            int _NumProjectedVerticies;
            float _ObjectRefractionIndex;

            sampler2D _MainTex;
            sampler2D _FinalLightColorTexture_0;
            float4 _MainTex_ST;
            
            float2 GetCoordinatesForSpecularTexture(float4x4 lightViewProjectionMatrix, float3 worldPos)
            {
                float4 texPt = mul(lightViewProjectionMatrix, float4(worldPos, 1));
                float2 tc = 0.5 * texPt.xy / texPt.w + float2(0.5, 0.5);
                return tc;
            }


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

                o.vertex = mul(UNITY_MATRIX_VP, float4(estimatedPosition, 1));
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = worldPos;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float4 sum = float4(0.0, 0.0, 0.0, 0.0);
                float radius = 15;
                float resolution = 1024;
                float blur = radius / resolution / 4;
                //float2 tc = i.uv;
                float _hstep = 0.5;
                float _vstep = 0.5;


                float2 tc = GetCoordinatesForSpecularTexture(_LightViewProjectionMatrix_0, i.worldPos);

                sum += tex2D(_FinalLightColorTexture_0, float2(tc.x - 4.0 * blur * _hstep, tc.y - 4.0 * blur * _vstep)) * 0.0162162162;
                sum += tex2D(_FinalLightColorTexture_0, float2(tc.x - 3.0 * blur * _hstep, tc.y - 3.0 * blur * _vstep)) * 0.0540540541;
                sum += tex2D(_FinalLightColorTexture_0, float2(tc.x - 2.0 * blur * _hstep, tc.y - 2.0 * blur * _vstep)) * 0.1216216216;
                sum += tex2D(_FinalLightColorTexture_0, float2(tc.x - 1.0 * blur * _hstep, tc.y - 1.0 * blur * _vstep)) * 0.1945945946;

                sum += tex2D(_MainTex, float2(tc.x, tc.y)) * 0.2270270270;

                sum += tex2D(_FinalLightColorTexture_0, float2(tc.x + 1.0 * blur * _hstep, tc.y + 1.0 * blur * _vstep)) * 0.1945945946;
                sum += tex2D(_FinalLightColorTexture_0, float2(tc.x + 2.0 * blur * _hstep, tc.y + 2.0 * blur * _vstep)) * 0.1216216216;
                sum += tex2D(_FinalLightColorTexture_0, float2(tc.x + 3.0 * blur * _hstep, tc.y + 3.0 * blur * _vstep)) * 0.0540540541;
                sum += tex2D(_FinalLightColorTexture_0, float2(tc.x + 4.0 * blur * _hstep, tc.y + 4.0 * blur * _vstep)) * 0.0162162162;

                // sample the texture
                float4 col = tex2D(_FinalLightColorTexture_0, tc);
                return col;
            }
            ENDCG
        }
    }
}
