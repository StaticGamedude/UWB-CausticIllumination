/*
* Shader responsible for creating the final shadow effect for light source 0. This shader is similar to the CausticFinalShaders except that
* it forces a refraction index of 1 (allowing light to pass through)
*/
Shader "Unlit/ShadowFinalShader_0"
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
                sampler2D _DrewCausticColor_0;
                sampler2D _CausticShadowTexture_0; // This contains the caustic flux and distance information

                float _GlobalAbsorbtionCoefficient;
                fixed4 _DebugLightColor_0;
                float _LightIntensity_0;
                float _AbsorbtionCoefficient;
                int _LightID;

                //Light specific parameters
                sampler2D _ReceivingPosTexture_0;
                float4x4 _LightViewProjectionMatrix_0;
                float3 _LightWorldPosition_0;
                float3 _LightCam_Forward_0;
                int _LightIsDirectional_0;

                v2f vert(appdata v)
                {
                    float2 uv = TRANSFORM_TEX(v.uv, _MainTex);
                    return SharedCausticFinalVertexShader(
                        v,
                        _LightViewProjectionMatrix_0,
                        _LightWorldPosition_0,
                        _LightCam_Forward_0,
                        _LightIsDirectional_0,
                        1,
                        uv,
                        _ReceivingPosTexture_0
                    );
                }

                fixed4 frag(v2f i) : SV_Target
                {
                    return SharedShadowFragmentShader(
                        i,
                        _LightViewProjectionMatrix_0,
                        _LightWorldPosition_0,
                        fixed4(0.8, 0.8, 0.8, 1),
                        _LightIntensity_0,
                        _CausticShadowTexture_0,
                        _AbsorbtionCoefficient,
                        _SpecularColorFactor
                    );
                }
                ENDCG
            }
        }
}