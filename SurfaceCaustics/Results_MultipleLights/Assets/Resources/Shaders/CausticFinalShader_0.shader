Shader "Unlit/CausticFinalShader_0"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
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
            sampler2D _DrewCausticColor_0;
            sampler2D _DrewTest_0; // This contains the caustic flux and distance information

            float _GlobalAbsorbtionCoefficient;
            fixed4 _DebugLightColor_0;
            float _LightIntensity_0;
            float _AbsorbtionCoefficient;
            int _LightID;

            //Light specific parameters
            sampler2D _ReceivingPosTexture_0;
            float4x4 _LightViewProjectionMatrix_0;
            float3 _LightWorldPosition_0;

            v2f vert (appdata v)
            {
                float2 uv = TRANSFORM_TEX(v.uv, _MainTex);
                return SharedCausticFinalVertexShader(
                    v,
                    _LightViewProjectionMatrix_0,
                    _LightWorldPosition_0,
                    _ObjectRefractionIndex,
                    uv,
                    _ReceivingPosTexture_0
                );
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return SharedCausticFinalFragmentShader(
                    i,
                    _LightViewProjectionMatrix_0,
                    _LightWorldPosition_0,
                    _DebugLightColor_0,
                    _LightIntensity_0,
                    _DrewTest_0,
                    _DrewCausticColor_0,
                    _AbsorbtionCoefficient,
                    _SpecularColorFactor
                );
            }
            ENDCG
        }
    }
}
