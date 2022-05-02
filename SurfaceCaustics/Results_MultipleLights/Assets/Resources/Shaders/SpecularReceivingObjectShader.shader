Shader "Unlit/SpecularReceivingObject"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "SpecularReceiver"="1"}
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
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            sampler2D _LightTexture0;
            sampler2D _LightTexture1;

            float4 _MainTex_ST;

            // Variables set globally from the CPU
            sampler2D _DrewCausticColor;
            sampler2D _CausticTexture;
            sampler2D _SpecularPosTexture;
            sampler2D _CausticFluxTexture;
            sampler2D _DrewTest;
            sampler2D _CausticMapTexture;
            sampler2D _CausticDistanceTexture;
            sampler2D _CausticColorMapTexture;
            sampler2D _FinalLightColorTexture_0;

            float4x4 _LightViewProjectionMatrix;
            float _IlluminationDistance;
            float _GlobalAbsorbtionCoefficient;
            float _DebugFlux;
            float3 _DiffuseObjectPos;
            float3 _LightWorldPosition;
            int _Debug_AllowNegativeIntensities;
            int _Debug_MultiplyIntensity;
            fixed4 _DebugLightColor;
            float _LightIntensity;
            float _AbsorbtionCoefficient;

            UNITY_DECLARE_TEX2DARRAY(_FinalLightingTextures);
            //sampler2D _FinalLightingTextures; //This contains an array of textures
            float _LightIDs[8];

            /*
            * Given the world position of the receiving object, get the texture
            * coordinates that can be used to map into a caustic texture.
            */
            float2 GetCoordinatesForSpecularTexture(float3 worldPos)
            {
                float4 texPt = mul(_LightViewProjectionMatrix, float4(worldPos, 1));
                float2 tc = 0.5 * texPt.xy / texPt.w + float2(0.5, 0.5);
                return tc;
            }

            /*
            * Determine if the world position (of the receiving object) is hit by any refracted
            * light ray
            */
            bool SpecularSeesPosition(float3 worldPos)
            {
                float2 tc = GetCoordinatesForSpecularTexture(worldPos);
                float4 fluxVals = tex2D(_DrewTest/*_CausticMapTexture*/, tc);
                return fluxVals.a == 1;
            }

            /*
            * Get the amount of flux that has accumulated onto the provided receiving position
            */
            float GetFlux(float3 worldPos)
            {
                float2 tc = GetCoordinatesForSpecularTexture(worldPos);
                float4 fluxVals = tex2D(_DrewTest, tc);
                return fluxVals.x;
            }

            float GetDistance(float3 worldPos)
            {
                float2 tc = GetCoordinatesForSpecularTexture(worldPos);
                float4 distanceVals = tex2D(_DrewTest, tc);
                return distanceVals.y;
            }

            fixed4 GetCausticColor(float3 worldPos)
            {
                float2 tc = GetCoordinatesForSpecularTexture(worldPos);
                fixed4 causticColor = tex2D(_DrewCausticColor, tc);
                return causticColor;
            }

            fixed4 GetFinalCausticColor(float3 worldPos, sampler2D lightTexture)
            {
                float2 tc = GetCoordinatesForSpecularTexture(worldPos);
                fixed4 causticColor = tex2D(_FinalLightColorTexture_0/*_LightTexture0*//*lightTexture*/, tc);
                return causticColor;
            }

            v2f vert (appdata v)
            {
                v2f o;
                float3 worldPos = mul(UNITY_MATRIX_M, v.vertex);
                float3 worldNormal = mul(transpose(unity_WorldToObject), v.normal);

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = worldPos;
                return o;
            }

            
            fixed4 frag (v2f i) : SV_Target
            {
                float flux = GetFlux(i.worldPos); //_DebugFlux;
                float d = GetDistance(i.worldPos);
                float finalIntensity = flux * exp((-/*_GlobalAbsorbtionCoefficient*/_AbsorbtionCoefficient * d));
                float2 tc = GetCoordinatesForSpecularTexture(i.worldPos);
                fixed4 col = tex2D(_MainTex, i.uv);
                fixed4 causticColor = GetCausticColor(i.worldPos);
                fixed4 finalColor = fixed4(0, 0, 0, 0);

                if (_LightIDs[0] != -1)
                {
                    finalColor = finalColor + GetFinalCausticColor(i.worldPos, _LightTexture0);
                }

                if (_LightIDs[1] != -1)
                {
                    finalColor = finalColor + GetFinalCausticColor(i.worldPos, _LightTexture1);
                }

                return finalColor;

                /*fixed4 finalColor = GetFinalCausticColor(i.worldPos);*/

                /*fixed4 firstSample = UNITY_SAMPLE_TEX2DARRAY(_FinalLightingTextures, float3(tc, _LightIDs[0]));
                for (int i = 1; i < 8; i++)
                {
                    if (_LightIDs[i] != -1)
                    {
                        firstSample = firstSample * UNITY_SAMPLE_TEX2DARRAY(_FinalLightingTextures, float3(tc, _LightIDs[i]));
                    }
                }

                return col * firstSample;*/

                //return col * finalColor;

                //if (finalIntensity >= 0 || (_Debug_AllowNegativeIntensities == 1))
                //{
                //    /*return col + finalIntensity;*/
                //    if (_Debug_MultiplyIntensity == 1)
                //    {
                //        return col * finalIntensity * _DebugLightColor * causticColor * _LightIntensity;
                //    }
                //    else
                //    {
                //        return col + finalIntensity;
                //    }
                //}

                //return col;
            }
            ENDCG
        }
    }
}