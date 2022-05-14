Shader "Unlit/SpecularReceivingObject"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _KernelSize("Kernel Size (N)", Int) = 3
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
            sampler2D _FinalLightColorTexture_1;
            sampler2D _CausticShadowTexture_0;
            sampler2D _CausticShadowTexture_1;
            sampler2D _ShadowFinalTexture_0;
            sampler2D _ShadowFinalTexture_1;
            sampler2D _TestTexture;
            sampler2D _CausticGaussianTexture_0;

            float4x4 _LightViewProjectionMatrix_0;
            float4x4 _LightViewProjectionMatrix_1;
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
            float _ShadowFactor;
            int _RenderShadows;
            int _RenderCaustics;
            float _ShadowThreshold;
            

            UNITY_DECLARE_TEX2DARRAY(_FinalLightingTextures);
            //sampler2D _FinalLightingTextures; //This contains an array of textures
            float _LightIDs[8];

            float2 _MainTex_TexelSize;
            int _KernelSize;
            int _CausticBlurKernalSize;


            /*
            * Given the world position of the receiving object, get the texture
            * coordinates that can be used to map into a caustic texture.
            */
            float2 GetCoordinatesForSpecularTexture(int lightID, float3 worldPos)
            {
                float4x4 projectionMatrix = lightID == 0 ? _LightViewProjectionMatrix_0 : _LightViewProjectionMatrix_1;
                float4 texPt = mul(projectionMatrix, float4(worldPos, 1));
                float2 tc = 0.5 * texPt.xy / texPt.w + float2(0.5, 0.5);
                return tc;
            }

            /*
            * Determine if the world position (of the receiving object) is hit by any refracted
            * light ray
            */
            bool SpecularSeesPosition(int lightID, float3 worldPos)
            {
                float2 tc = GetCoordinatesForSpecularTexture(lightID, worldPos);
                float4 fluxVals = tex2D(_DrewTest/*_CausticMapTexture*/, tc);
                return fluxVals.a == 1;
            }

            /*
            * Get the amount of flux that has accumulated onto the provided receiving position
            */
            float GetFlux(int lightID, float3 worldPos)
            {
                float2 tc = GetCoordinatesForSpecularTexture(lightID, worldPos);
                float4 fluxVals = tex2D(_DrewTest, tc);
                return fluxVals.x;
            }

            float GetDistance(int lightID, float3 worldPos)
            {
                float2 tc = GetCoordinatesForSpecularTexture(lightID, worldPos);
                float4 distanceVals = tex2D(_DrewTest, tc);
                return distanceVals.y;
            }

            fixed4 GetCausticColor(int lightID, float3 worldPos)
            {
                float2 tc = GetCoordinatesForSpecularTexture(lightID, worldPos);
                fixed4 causticColor = tex2D(_DrewCausticColor, tc);
                return causticColor;
            }

            fixed4 GetFinalCausticColor(int lightID, float3 worldPos, sampler2D lightTexture)
            {
                float2 tc = GetCoordinatesForSpecularTexture(lightID, worldPos);
                fixed4 causticColor = tex2D(lightTexture, tc);
                return causticColor;
            }

            bool IsShadowPosition(int lightID, float3 worldPos, sampler2D shadowTexture)
            {
                float2 tc = GetCoordinatesForSpecularTexture(lightID, worldPos);
                fixed4 shadowColor = tex2D(shadowTexture, tc);
                if (shadowColor.r > _ShadowThreshold && shadowColor.g > _ShadowThreshold && shadowColor.b > _ShadowThreshold)
                {
                    return true;
                }

                return false;
            }

            bool IsUnderLightShadowCam(int lightID, sampler2D shadowTexture, float3 worldPos)
            {
                if (_LightIDs[lightID] == -1)
                {
                    return false;
                }

                return IsShadowPosition(lightID, worldPos, shadowTexture);
            }

            fixed4 BlurTexture(sampler2D tex, float2 texCoordinate)
            {
                fixed4 sum = fixed4(0.0, 0.0, 0.0, 0.0);

                // upper and low define our range. We got left by X pixels, and right by the same X pixels.
                // Likewise for up and down.
                int upperBound = ((_CausticBlurKernalSize - 1) / 2);
                int lowBound = -upperBound;

                for (int x = lowBound; x <= upperBound; ++x)
                {
                    for (int y = lowBound; y <= upperBound; ++y)
                    {
                        // _MainTex_TexelSize is populated by Unity
                        fixed2 offset = fixed2(_MainTex_TexelSize.x * x, _MainTex_TexelSize.y * y);

                        // Accumulate the total color
                        sum += tex2D(tex, texCoordinate + offset);
                    }
                }

                // Divide the size 
                sum /= (_CausticBlurKernalSize * _CausticBlurKernalSize);
                return sum;

                /*fixed3 sum = fixed3(0.0, 0.0, 0.0);

                int upper = ((_CausticBlurKernalSize - 1) / 2);
                int lower = -upper;

                for (int x = lower; x <= upper; ++x)
                {
                    for (int y = lower; y <= upper; ++y)
                    {
                        fixed2 offset = fixed2(_MainTex_TexelSize.x * x, _MainTex_TexelSize.y * y);
                        sum += tex2D(tex, texCoordinate + offset);
                    }
                }

                sum /= (_CausticBlurKernalSize * _CausticBlurKernalSize);
                return fixed4(sum, 1.0);*/
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
                fixed4 col = tex2D(_MainTex, i.uv);
                fixed4 finalColor = fixed4(0, 0, 0, 0);

                if (_RenderCaustics == 1)
                {
                    float2 tc = GetCoordinatesForSpecularTexture(0, i.worldPos);
                    if (_LightIDs[0] != -1)
                    {
                        finalColor = finalColor + BlurTexture(_FinalLightColorTexture_0, tc);
                    }

                    if (_LightIDs[1] != -1)
                    {
                        finalColor = finalColor + BlurTexture(_FinalLightColorTexture_1, tc);
                    }
                }
                
                /*float2 tc = GetCoordinatesForSpecularTexture(0, i.worldPos);
                fixed4 shadowColor = tex2D(_ShadowFinalTexture_0, tc);*/
                /*if (_RenderShadows == 1 && (IsUnderLightShadowCam(0, _ShadowFinalTexture_0, i.worldPos) || IsUnderLightShadowCam(1, _ShadowFinalTexture_1, i.worldPos)))
                {
                    col = col * 0.4;
                }*/

                /*fixed4 testCol = tex2D(_TestTexture, tc);
                return testCol;*/

                /*if (_RenderShadows == 1)
                {
                    col = col - shadowColor;
                }*/

                return col + finalColor;
            }
            ENDCG
        }
    }
}
