Shader "Unlit/WaveShader"
{
    Properties
    {
        _Amplitude ("Wave Amplitude", Range(0, 10)) = 0.1
        _Frequency ("Wave Amplitude", Range(0, 10)) = 0.1
        _AnimationSpeed ("Wave Amplitude", Range(0, 10)) = 0.1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Cull OFF

        Pass
        {
            Tags { "LightMode"="ObjectA" }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            #define TAU 6.28318530718

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent: TANGENT;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 normal : TEXCOORD0;
                //float2 uv : TEXCOORD1;
                float dist : float;
                float4 tangent : TEXCOORD10;
                float4 bitangent: TEXCOORD11;
                float3 ntangent: TEXCOORD12;
            };

            float _Amplitude;
            float _Frequency;
            float _AnimationSpeed;


            /*
            float3 waveform (float3 base)
            {
                sin(posPlusTangent.x + _Time.y) * _Amp;
            }
            */

            v2f vert (appdata v)
            {
                v2f o;

                // recaculate normals            
                // Obtain tangent space rotation matrix
               // float3 binormal = cross( v.normals, v.tangent.xyz ) * v.tangent.w;
                //float3x3 rotation = transpose( float3x3( v.tangent.xyz, binormal, v.normals ) );
           
                // Create two sample vectors (small +x/+y deflections from +z), put them in tangent space, normalize them, and halve the result.
                // This is equivalent to sampling neighboring vertex data since we're on a unit sphere.
                //float3 v1 = normalize( mul( rotation, float3(0.1, 0, 1) ) ) * 0.5;
               // float3 v2 = normalize( mul( rotation, float3(0, 0.1, 1) ) ) * 0.5;

                // ref https://www.youtube.com/watch?v=kfM-yu0iQBk&t=11110s
                //float wave = cos( (v.uv.y - _Time.y * 0.1) * TAU * 5);
                //v.vertex.y = wave * _Amp;

                //v1.y = cos( (v1.y - _Time.y * 0.1) * TAU * 5) * _Amp;
                //v2.y = cos( (v2.y - _Time.y * 0.1) * TAU * 5) * _Amp;

                //float3 vn = cross( v2-v.vertex.xyz, v1-v.vertex.xyz );
                //o.normals = normalize( vn );

                /*
                float4 modifiedPos = v.vertex;
                modifiedPos.y = cos( (v.uv.y - _Time.y * 0.1) * TAU * 5) * _Amp;
                float3 bitangent = cross(v.normals, v.tangent.xyz);
                
                float3 posPlusTangent = v.vertex + v.tangent.xyz * 0.05;
                posPlusTangent.y += sin(posPlusTangent.x + _Time.y) * _Amp;

                float3 posPlusBitangent = v.vertex + bitangent * 0.05;
                posPlusBitangent.y += sin(posPlusBitangent.x + _Time.y) * _Amp;

                float3 modifiedTangent = posPlusTangent - modifiedPos;
                float3 modifiedBitangent = posPlusBitangent - modifiedPos;

                float3 modifiedNormal = cross(modifiedTangent, modifiedBitangent);
                o.normals = UnityObjectToWorldNormal(normalize(modifiedNormal));
                o.normals = bitangent;
                
                o.tangent =  v.tangent; //should be black
                o.bitangent =  float4(bitangent,1); //should be blue
                    
                o.vertex = UnityObjectToClipPos(modifiedPos);
                //o.vertex = modifiedPos;
                //o.normals = UnityObjectToWorldNormal(v.normals);
                o.uv = v.uv;
                */

                //o.vertex = UnityObjectToClipPos(v.vertex);

                // ref: https://www.ronja-tutorials.com/post/015-wobble-displacement/
                o.vertex = v.vertex;
                o.vertex.y += sin(v.vertex.x * _Frequency + _Time.y * _AnimationSpeed) * _Amplitude;
                float3 distval = o.vertex;

                o.tangent = v.tangent;
                float3 posPlusTangent = v.vertex + v.tangent * 0.01;
                posPlusTangent.y += sin(posPlusTangent.x * _Frequency + _Time.y * _AnimationSpeed) * _Amplitude;

                float3 bitangent = cross(v.normal, v.tangent);
                o.bitangent = float4(bitangent, 1);
                float3 posPlusBitangent = v.vertex + bitangent * 0.01;
                posPlusBitangent.y += sin(posPlusBitangent.x * _Frequency + _Time.y * _AnimationSpeed) * _Amplitude;

                float3 modifiedTangent = posPlusTangent - o.vertex;
                float3 modifiedBitangent = posPlusBitangent - o.vertex;

                float3 modifiedNormal = cross(modifiedTangent, modifiedBitangent);
                o.normal = UnityObjectToWorldNormal(normalize(modifiedNormal));

                o.ntangent = normalize(modifiedTangent);

                o.vertex = UnityObjectToClipPos(o.vertex);

                o.dist = length(WorldSpaceViewDir(float4(distval, v.vertex.w)));
                return o;
            }

            void frag (v2f i,
            out half4 GRT0:SV_Target0,
            out half4 GRT1:SV_Target1,
            out half4 GRT3:SV_TARGET3
            )
			{   // depth : d1 color (wave to camera)             
                half toColor = i.dist/30;
				GRT0 = fixed4(toColor, toColor, toColor,1);
                // normal
                // fixed color for removing black color - was 0.5f for the factor
                float3 fixedcolor = i.normal + float3(1,1,1) * 0.8f;
                GRT1 = fixed4(fixedcolor, 1);
                // new tangent
                GRT3 = fixed4(i.ntangent, 1);
			}
            ENDCG
        }
    }
}
