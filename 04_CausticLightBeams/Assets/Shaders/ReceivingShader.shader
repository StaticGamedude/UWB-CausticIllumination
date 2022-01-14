Shader "Unlit/ReceivingShader"
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

            //float4x4 _SpecularPosTexture;
            //float4x4 _SpecularNormalTexture;
            
            

            //sampler2D _LightPosTexture; //Set externally by shadow caster script
            //sampler2D _LightNormTexture; //Set externally by shadow caster script

            v2f vert (appdata v)
            {
                v2f o;
                float3 worldPos = mul(UNITY_MATRIX_M, v.vertex);


                o.lightPosCoords = mul(_LightMatrix, float4(worldPos, 1.0));
                o.lightPosCoords.z = -mul(_LightCamMatrix, float4(worldPos, 1.0)).z * _LightCam_Far; // viewspace Z

                o.lightNormCoords = mul(_LightMatrix, float4(worldPos, 1.0));
                o.lightNormCoords.z = -mul(_LightCamMatrix, float4(worldPos, 1.0)).z * _LightCam_Far; // viewspace Z

                o.worldPos = worldPos;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                
                float4 lightPos = tex2D(_SpecularPosTexture, i.lightPosCoords.xy / i.lightPosCoords.w);
                float4 lightNorm = tex2D(_SpecularNormalTexture, i.lightNormCoords.xy / i.lightNormCoords.w);
                float3 lightPosReduced = float3(lightPos.r, lightPos.g, lightPos.b);
                float3 lightNormReduced = float3(lightNorm.r, lightNorm.g, lightNorm.b);
                float3 worldPos = i.worldPos;
                float3 lightPosToWorldPos = worldPos - lightPosReduced;

                float3 lightNormNormalized = normalize(lightNormReduced);
                float3 lightPosToWorldPosNormalized = normalize(lightPosToWorldPos);

                float angleBetween = abs(acos(dot(lightNormNormalized, lightPosToWorldPosNormalized)));

                if (angleBetween < 2)
                {
                    fixed4 col = fixed4(1, 0, 0, 1);
                    return col;
                }
                else
                {
                    fixed4 col = fixed4(0, 0, 0, 1);
                    return col;
                }

                /*fixed4 col = fixed4(0, 0, 0, 1);
                return col;*/
            }
            ENDCG
        }
    }
}
