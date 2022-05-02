Shader "Unlit/CausticFinalShader"
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

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "CausticFinalFunctions.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _ObjectRefractionIndex;
            // Variables set globally from the CPU
            sampler2D _DrewCausticColor;
            sampler2D _CausticTexture;
            sampler2D _SpecularPosTexture;
            sampler2D _CausticFluxTexture;
            sampler2D _DrewTest;
            sampler2D _CausticMapTexture;
            sampler2D _CausticDistanceTexture;
            sampler2D _CausticColorMapTexture;

            float _GlobalAbsorbtionCoefficient;
            fixed4 _DebugLightColor;
            float _LightIntensity;
            float _AbsorbtionCoefficient;
            int _LightID;

            //Light specific parameters
            sampler2D _ReceivingPosTexture;
            float4x4 _LightViewProjectionMatrix;
            float3 _LightWorldPosition;

            v2f vert (appdata v)
            {
                float2 uv = TRANSFORM_TEX(v.uv, _MainTex);
                return SharedCausticFinalVertexShader(
                    v,
                    _LightViewProjectionMatrix,
                    _LightWorldPosition,
                    _ObjectRefractionIndex,
                    uv,
                    _ReceivingPosTexture
                );
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return SharedCausticFinalFragmentShader(
                    i,
                    _LightViewProjectionMatrix,
                    _DebugLightColor,
                    _LightIntensity,
                    _DrewTest,
                    _DrewCausticColor,
                    _AbsorbtionCoefficient
                );
            }
            ENDCG
        }
    }
}
