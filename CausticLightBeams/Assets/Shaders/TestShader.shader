Shader "Unlit/TestShader"
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
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 lightCoords: TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _SpecularPosTexture;
            float4x4 _LightMatrix;

            v2f vert(appdata v)
            {
                v2f o;
                
                float3 worldPos = mul(UNITY_MATRIX_M, v.vertex);
                float4 lightCoords = mul(_LightMatrix, float4(worldPos, 1.0));


                
                o.lightCoords = lightCoords;

                // Default behavior
                float4 desiredPos = UnityObjectToClipPos(v.vertex);

                // Line below fails - tex2D seems to be only available in the fragment shader
                //float4 desiredPos = tex2D(_SpecularPosTexture, lightCoords.xy / lightCoords.w);

                // This line compiles okay, but can't seem to find where the geometry has gone to in the scene
                //float4 desiredPos = tex2Dlod(_SpecularPosTexture, float4(lightCoords.xy, lightCoords.w, 0));

                o.vertex = desiredPos;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Simply color the sphere to the expected pos value - similar to what we did for the shadow mapping
                float r = tex2D(_SpecularPosTexture, i.lightCoords.xy / i.lightCoords.w).r;
                float g = tex2D(_SpecularPosTexture, i.lightCoords.xy / i.lightCoords.w).g;
                float b = tex2D(_SpecularPosTexture, i.lightCoords.xy / i.lightCoords.w).b;

                // sample the texture
                return fixed4(r, g, b, 1);
            }
            ENDCG
        }
    }
}
