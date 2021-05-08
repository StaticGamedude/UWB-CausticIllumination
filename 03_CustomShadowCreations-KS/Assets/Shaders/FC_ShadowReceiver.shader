Shader "Unlit/FC_ShadowReceiver"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
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
                float4 shadowCoords: TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4x4 _FC_ShadowMatrix; //Set externally by shadow caster script
            float4x4 _FC_ShadowCamMatrix;  // To Shadow Cam's View Space
            float _FC_ShadowCam_Far; // 1 over shadow cam.farClipPlane
            sampler2D _FC_ShadowTex; //Set externally by shadow caster script
            float _FC_ShadowBias; //Set externally by shadow caster script

            v2f vert (appdata v)
            {
                v2f o;
                float3 worldPos = mul(UNITY_MATRIX_M, v.vertex);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.shadowCoords = mul(_FC_ShadowMatrix, float4(worldPos, 1.0));
                o.shadowCoords.z = -mul(_FC_ShadowCamMatrix, float4(worldPos, 1.0)).z * _FC_ShadowCam_Far; // viewspace Z
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // *** Original code
                //float lightDepth = 1 - tex2D(_FC_ShadowTex, i.shadowCoords.xy / i.shadowCoords.w).r; //Relying on color to get depth information. Only once channel needed
                //float shadow = (i.shadowCoords.z - _FC_ShadowBias) < lightDepth ? 1.0 : 0.5;
                //fixed4 col = tex2D(_MainTex, i.uv);
                //col = col * shadow;
                //return col;


                // This should be correct?
                float lightDepth = tex2D(_FC_ShadowTex, i.shadowCoords.xy / i.shadowCoords.w).r; //Relying on color to get depth information. Only once channel needed
                float myD = i.shadowCoords.z;
                float shadow = (myD < lightDepth) ? 0.8 : 0.4;
                return shadow;


                // verify the z values make sense! This is the problem now
                //      Select the plane and change the plane's Y-value to see the color change
                //      It makes sense.
                /*float d = i.shadowCoords.z;
                float g = (d > 1.0) ? 1.0 : 0.0;
                float r = (d < 1.0) ? 1.0 : 0.0;
                float b = (d < 0.5) ? 1.0 : 0.0;
                return fixed4(r, g, b, 1.0);*/

                // Verifies the shadow map (the smaller the closer)
                //float lightDepth = tex2D(_FC_ShadowTex, i.shadowCoords.xy / i.shadowCoords.w).r; //Relying on color to get depth information. Only once channel needed
                //return fixed4(lightDepth, lightDepth, lightDepth, 1);

                // What is this?
                //float lightDepth = 1 - tex2D(_FC_ShadowTex, i.shadowCoords.xy / i.shadowCoords.w).r; //Relying on color to get depth information. Only once channel needed
                //float shadow = lightDepth < 0.5 ? 0.5 : 1;
                //return fixed4(shadow, shadow, shadow, 1);
            }
            ENDCG
        }
    }
}
