/*
* Simple shader which is used on items that should be considered specular objects.
* The primary difference between this shader and a basic unlit shader is the use of the 
* "SpecularObj" tag in the shader.
*/
Shader "Unlit/SpecularObjectShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

        /*
        * Note: The parameters below are not actually used here. Rather, they are picked up
        * by the replacement shaders that are rendering the same object.
        */
        _ObjectRefractionIndex ("Refraction Index", Float) = 1.0
        _AbsorbtionCoefficient ("Absorbtion Coefficient", Float) = 0.00017
        _SpecularColorFactor ("Specular Color Factor (0-1)", Float) = 1
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "SpecularObj" = "1" }
        LOD 100
        //Blend SrcAlpha OneMinusSrcAlpha

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
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD1;
                float3 normal : TEXDCOORD2;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float _AllLightIds[8];

            float3 _LightWorldPosition_0;
            float3 _LightCam_Forward_0;
            float _LightIntensity_0;
            int _LightIsDirectional_0;

            float3 _LightWorldPosition_1;
            float3 _LightCam_Forward_1;
            float _LightIntensity_1;
            int _LightIsDirectional_1;


            bool IsPositionVisibleByLightSource(float3 worldPos, float3 worldNormal)
            {
                float3 lightPositions[2];

                lightPositions[0] = _LightWorldPosition_0;
                lightPositions[1] = _LightWorldPosition_1;

                for (int i = 0; i < 2; i++)
                {
                    float3 lightPos = lightPositions[i];
                    float3 pointToLight = normalize(lightPos - worldPos);
                    if (dot(pointToLight, worldNormal) > 0)
                    {
                        return true;
                    }
                }

                return false;
            }

            float GetLightMultiplier(
                int isDirectional, 
                float3 lightWorldPos, 
                float3 worldPos,
                float3 lightForward, 
                float3 vertexNormal,
                float lightIntensity)
            {
                float3 pointToLight = normalize(lightWorldPos - worldPos);
                float3 lightDir = isDirectional == 1 ? -lightForward : pointToLight;
                return dot(lightDir, vertexNormal);
            }

            v2f vert (appdata v)
            {
                v2f o;
                float3 worldPos = mul(UNITY_MATRIX_M, v.vertex);
                float3 worldNormal = normalize(mul(transpose(unity_WorldToObject), v.normal));
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.normal = worldNormal;
                o.worldPos = worldPos;
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float3 pointToLight1 = normalize(_LightWorldPosition_0 - i.worldPos);
                float3 lightDir = _LightIsDirectional_0 ? _LightCam_Forward_0 : pointToLight1;
                float4 col = tex2D(_MainTex, i.uv);
                float lightMultiplier = 0;

                if (_AllLightIds[0] != -1)
                {
                    lightMultiplier += GetLightMultiplier(_LightIsDirectional_0, _LightWorldPosition_0, i.worldPos, _LightCam_Forward_0, i.normal, _LightIntensity_0);
                }

                if (_AllLightIds[1] != -1)
                {
                    lightMultiplier += GetLightMultiplier(_LightIsDirectional_1, _LightWorldPosition_1, i.worldPos, _LightCam_Forward_1, i.normal, _LightIntensity_1);
                }

                float4 resultingColor = col + (col * lightMultiplier);

                return float4(resultingColor.xyz, 0.5);


                //return col + (col * lightMultiplier);
            }
            ENDCG
        }
    }

    Fallback "Diffuse"
}
