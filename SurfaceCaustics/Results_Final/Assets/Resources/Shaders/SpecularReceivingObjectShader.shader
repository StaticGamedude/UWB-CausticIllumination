/*
* Handles the final rendering of the specular object. Takes into account the caustic effect textures for each light sources and uses
* them to determine the final color of each fragment.
*/
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
                float3 normal : TEXCOORD2;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float2 _MainTex_TexelSize;

            // Variables set globally from the CPU
            float _LightIDs[8];
            float _AllLightIds[8];

            sampler2D _FinalLightColorTexture_0;
            sampler2D _FinalLightColorTexture_1;
            sampler2D _CausticShadowTexture_0;
            sampler2D _CausticShadowTexture_1;
            sampler2D _ShadowFinalTexture_0;
            sampler2D _ShadowFinalTexture_1;
            float3 _LightWorldPosition_0;
            float3 _LightWorldPosition_1;
            float4x4 _LightViewProjectionMatrix_0;
            float4x4 _LightViewProjectionMatrix_1;
            int _RenderShadows;
            int _RenderCaustics;
            float _ShadowThreshold;
            float _CausticThreshold;
            int _CausticBlurKernalSize;
            int _ShadowBlurKernelSize;
            
            /*
            * Given the world position of the receiving object, get the texture
            * coordinates that can be used to map into a caustic texture.
            * param: lightID - The unique light source ID
            * param: worldPos - The receiving object vertex world position
            */
            float2 GetCoordinatesForSpecularTexture(int lightID, float3 worldPos)
            {
                float4x4 projectionMatrix = lightID == 0 ? _LightViewProjectionMatrix_0 : _LightViewProjectionMatrix_1;
                float4 texPt = mul(projectionMatrix, float4(worldPos, 1));
                float2 tc = 0.5 * texPt.xy / texPt.w + float2(0.5, 0.5);
                return tc;
            }

            /*
            * Gets a blurred reulst from a colored texture. 
            * param: tex - The texture to get color from
            * param: texCoordinate - The texture coordinate used to index into the source texture
            * param: kernel size - The kernel size used for the Gaussian blur
            */
            fixed4 BlurTexture(sampler2D tex, float2 texCoordinate, int kernelSize)
            {
                fixed4 sum = fixed4(0.0, 0.0, 0.0, 0.0);

                // upper and low define our range. We got left by X pixels, and right by the same X pixels.
                // Likewise for up and down.
                int upperBound = ((kernelSize - 1) / 2);
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
                sum /= (kernelSize * kernelSize);
                return sum;
            }

            /*
            * Utility method to determine if a color exceeds the provided threshold
            * param: color - Color value to inspect
            * param: threshold - Desired color value limit/threshold
            * param: checkAll - A flag used in indicate whether all rgba values should be checked or if just the alpha can be checcked
            */
            bool IsColorGreaterThanThreshold(fixed4 color, float threshold, bool checkAll)
            {
                if (checkAll)
                {
                    return color.r > threshold && color.g > threshold && color.b > threshold && color.a > threshold;
                }

                return color.a > threshold;
            }

            /*
            * Utility method to determine if a receiving position is visible by a light source
            */
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

            v2f vert (appdata v)
            {
                v2f o;
                float3 worldPos = mul(UNITY_MATRIX_M, v.vertex);
                float3 worldNormal = mul(transpose(unity_WorldToObject), v.normal);

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = worldPos;
                o.normal = normalize(mul(transpose(unity_WorldToObject), v.normal));
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                fixed4 finalColor = fixed4(0, 0, 0, 0);

                float2 causticCamera0TexCoord = GetCoordinatesForSpecularTexture(0, i.worldPos);
                float2 causticCamera1TexCoord = GetCoordinatesForSpecularTexture(1, i.worldPos);
                
                if (_AllLightIds[0] != -1)
                {
                    finalColor = finalColor + BlurTexture(_FinalLightColorTexture_0, causticCamera0TexCoord, _CausticBlurKernalSize);
                }

                if (_AllLightIds[1] != -1)
                {
                    finalColor = finalColor + BlurTexture(_FinalLightColorTexture_1, causticCamera1TexCoord, _CausticBlurKernalSize);
                }
                
                fixed4 shadowColor = BlurTexture(_ShadowFinalTexture_0, causticCamera0TexCoord, _ShadowBlurKernelSize);

                // Check to see if the this spot can be seen by our light source. If not, simply return the color.
                if (!IsPositionVisibleByLightSource(i.worldPos, i.normal))
                {
                    return col;
                }

                

                // Shadow attempt based on the algorithm mentioned here: https://citeseerx.ist.psu.edu/viewdoc/download?doi=10.1.1.625.7037&rep=rep1&type=pdf
                // Check to see if this spot has a caustic effect on it. If so, simply return the normal color plus the caustic effect.
                // If not, check to see if it has a shadow effect. If so, darken the spot.
                //if (_RenderCaustics == 1 && IsColorGreaterThanThreshold(finalColor, _CausticThreshold, true))
                //{
                //    return col + finalColor;
                //}
                //
                //if (_RenderShadows == 1 && IsColorGreaterThanThreshold(shadowColor, _ShadowThreshold, true))
                //{
                //    return col * 0.8;
                //    /*return fixed4(0, 0, 0, 1);*/
                //}

                // My attempt for supporting shadows. If the shadow value within a certain theshold, darken the color. Then add the caustic effect on top
                if (_RenderShadows == 1 && IsColorGreaterThanThreshold(shadowColor, _ShadowThreshold, true))
                {
                    col = col * 0.8;
                }

                if (_RenderCaustics == 1)
                {
                    return col + finalColor;
                }

                return col;
            }
            ENDCG
        }
    }

    Fallback "Diffuse"
}
