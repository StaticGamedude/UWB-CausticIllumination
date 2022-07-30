/*
* Computes the final caustic result for light source 1. Most of the operational logic for this shader is found in
* CausticFinalFunctions.cginc
*/
Shader "Unlit/CausticFinalShader_1"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

        /*Deprecated parameter - not ultimately used*/
        _LightID ("Light ID", Int) = 0
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
            sampler2D _DrewTest_1; // This contains the caustic flux and distance information

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

            v2f vert (appdata v)
            {
                float2 uv = TRANSFORM_TEX(v.uv, _MainTex);
                return SharedCausticFinalVertexShader(
                    v,
                    _LightViewProjectionMatrix_1,
                    _LightWorldPosition_1,
                    _LightCam_Forward_1,
                    _LightIsDirectional_1,
                    _ObjectRefractionIndex,
                    uv,
                    _ReceivingPosTexture_1
                );
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return SharedCausticFinalFragmentShader(
                    i,
                    _LightViewProjectionMatrix_1,
                    _LightWorldPosition_1,
                    _DebugLightColor_1,
                    _LightIntensity_1,
                    _DrewTest_1,
                    _DrewCausticColor_1,
                    _AbsorbtionCoefficient,
                    _SpecularColorFactor
                );
            }
            ENDCG
        }
    }
}
