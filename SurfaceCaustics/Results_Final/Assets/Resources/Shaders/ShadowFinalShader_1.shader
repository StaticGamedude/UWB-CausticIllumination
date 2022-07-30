/*
* Shader responsible for creating the final shadow effect for light source 1. This shader is similar to the CausticFinalShaders except that
* it forces a refraction index of 1 (allowing light to pass through)
*/
Shader "Unlit/ShadowFinalShader_1"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _LightID("Light ID", Int) = 0
    }
        SubShader
        {
            Tags { "SpecularObj" = "1" }
            LOD 100    
            Cull Off

            Pass
            {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag

                #include "CausticFinalFunctions.cginc"

                sampler2D _MainTex;
                float4 _MainTex_ST;
                float _ObjectRefractionIndex;
                float _SpecularColorFactor;
                // Variables set globally from the CPU
                sampler2D _DrewCausticColor_1;
                sampler2D _CausticShadowTexture_1; // This contains the caustic flux and distance information

                float _GlobalAbsorbtionCoefficient;
                fixed4 _DebugLightColor_1;
                float _LightIntensity_1;
                float _AbsorbtionCoefficient;
                int _LightID;

                //Light specific parameters
                sampler2D _ReceivingPosTexture_1;
                float4x4 _LightViewProjectionMatrix_1;
                float3 _LightWorldPosition_1;
                float3 _LightCam_Forward_1;
                int _LightIsDirectional_1;

                v2f vert(appdata v)
                {
                    float2 uv = TRANSFORM_TEX(v.uv, _MainTex);
                    return SharedCausticFinalVertexShader(
                        v,
                        _LightViewProjectionMatrix_1,
                        _LightWorldPosition_1,
                        _LightCam_Forward_1,
                        _LightIsDirectional_1,
                        1,
                        uv,
                        _ReceivingPosTexture_1
                    );
                }

                fixed4 frag(v2f i) : SV_Target
                {
                    return SharedShadowFragmentShader(
                        i,
                        _LightViewProjectionMatrix_1,
                        _LightWorldPosition_1,
                        fixed4(0.8, 0.8, 0.8, 0.8),
                        _LightIntensity_1,
                        _CausticShadowTexture_1,
                        _AbsorbtionCoefficient,
                        _SpecularColorFactor
                    );
                }
                ENDCG
            }
        }
}
