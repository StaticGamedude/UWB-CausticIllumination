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
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            // Variables set globally from the CPU
            sampler2D _CausticTexture;
            sampler2D _CausticFluxTexture;
            sampler2D _SpecularPosTexture;
            float4x4 _LightViewProjectionMatrix;
            float _IlluminationDistance;
            float3 _DiffuseObjectPos;

            float GetCausticIntensity(float3 worldPos)
            {
                float totalFlux = 0;
                for (int i = 0; i < 20; i++)
                {
                    for (int j = 0; j < 20; j++)
                    {
                        float xCoord = i / 20.0;
                        float yCoord = j / 20.0;
                        float2 textureCoord = float2(xCoord, yCoord);

                        float4 flux = tex2D(_CausticFluxTexture, textureCoord);
                        float4 splatPos = tex2D(_CausticTexture, textureCoord);
                        float4 specularPos = tex2D(_SpecularPosTexture, textureCoord);
                        if (specularPos.a != 0)
                        {
                            float d = distance(worldPos, splatPos);
                            if (d < _IlluminationDistance)
                            {
                                totalFlux += flux;
                            }
                        }
                    }
                }

                return totalFlux;
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
                /*float4 totalFlux = GetCausticIntensity(i.worldPos);
                float absorbtionCoef = pow(10, -5);
                float d = distance(i.worldPos, _DiffuseObjectPos);
                float finalIntensity = totalFlux * exp((-absorbtionCoef * d));


                return col * finalIntensity;*/

                return col;

                //if (causticIntensity.a > 0)
                //{
                //    //return col * causticIntensity.r;
                //    return fixed4(col.r + causticIntensity.r, col.g + causticIntensity.r, col.b + causticIntensity.r, col.a);
                //}
                //else
                //{
                //    return col;
                //}
            }
            ENDCG
        }
    }
}
