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
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 lightPosCoords: TEXCOORD1;
                float4 lightNormCoords: TEXCOORD2;
                float3 worldPos : TEXCOORD3;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            // Variables set globally from the CPU
            sampler2D _SpecularPosTexture;
            sampler2D _SpecularNormalTexture;
            float4x4 _LightMatrix;
            float4x4 _LightCamMatrix;
            float _LightCam_Far;

            v2f vert (appdata v)
            {
                v2f o;
                float3 worldPos = mul(UNITY_MATRIX_M, v.vertex);

                //Convert the world position into the light's space. The LightMatrix consist of the bias * projection matrix * camera matrix
                o.lightPosCoords = mul(_LightMatrix, float4(worldPos, 1.0));

                //TODO: Ask Dr. Sung about this line - the depth of the position is based on how far the camera can see - camera matrix * world position * 1/far clipping plane
                o.lightPosCoords.z = -mul(_LightCamMatrix, float4(worldPos, 1.0)).z * _LightCam_Far; // viewspace Z

                //Convert the world normal into the light's space. The LightMatrix consist of the bias * projection matrix * camera matrix
                o.lightNormCoords = mul(_LightMatrix, float4(worldPos, 1.0));

                //TODO: Ask Dr. Sung about this line - the depth of the position is based on how far the camera can see - camera matrix * world position * 1/far clipping plane
                o.lightNormCoords.z = -mul(_LightCamMatrix, float4(worldPos, 1.0)).z * _LightCam_Far; // viewspace Z

                o.worldPos = worldPos;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Index into the position and normal textures to get the corresponding light position and normal
                float4 lightPos = tex2D(_SpecularPosTexture, i.lightPosCoords.xy / i.lightPosCoords.w);
                float4 lightNorm = tex2D(_SpecularNormalTexture, i.lightNormCoords.xy / i.lightNormCoords.w);

                // Convert the values to a float3 (from a float 4)
                float3 lightPosReduced = float3(lightPos.r, lightPos.g, lightPos.b);
                float3 lightNormReduced = float3(lightNorm.r, lightNorm.g, lightNorm.b);

                float3 worldPos = i.worldPos;

                // Get a vector from the light position to the world position of the fragment we're trying to render
                float3 lightPosToWorldPos = worldPos - lightPosReduced;

                // Normalize our light pos to world position and our light normal so we can obtain an accurate angle between them
                float3 lightNormNormalized = normalize(lightNormReduced); //Do we need to normalize this again? Wouldn't it be normalized already?
                float3 lightPosToWorldPosNormalized = normalize(lightPosToWorldPos);
                
                // Calculate the angle between the vectors
                float angleBetween = abs(acos(dot(lightNormNormalized, lightPosToWorldPosNormalized)));

                // What's the minimum amount 
                float angleNeededForIncreasedIllumination = radians(50);

                // If the angle between is with the desired number of degrees, increase the illumination of the fragment
                //if (angleBetween < angleNeededForIncreasedIllumination)
                //{
                //    float lightIlluminationIncreaseAmount = 0.15;
                //    fixed4 col = tex2D(_MainTex, i.uv);
                //    col.r = col.r + lightIlluminationIncreaseAmount;
                //    col.g = col.g + lightIlluminationIncreaseAmount;
                //    col.b = col.b + lightIlluminationIncreaseAmount;
                //    return col;
                //}
                //else
                //{
                //    // sample the texture
                //    fixed4 col = tex2D(_MainTex, i.uv);
                //    return col;
                //}

                return fixed4(1, 1, 1, 1);
            }
            ENDCG
        }
    }
}
