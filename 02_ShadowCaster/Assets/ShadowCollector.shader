/*
 * This implementation was developed by anyone on the UWB Casutic 
 * illumniation team. Rather this solution was developed by user Curious-George
 * and made public available through the unity forums. The following page was 
 * accessed 02/20/2021: https://forum.unity.com/threads/how-do-i-render-my-own-shadow-map.471293/
 */
Shader "Custom/ShadowCollector" {
	Properties {
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
        #include "UnityCG.cginc"
		#pragma surface surf Lambert fullforwardshadows vertex:vert
		#pragma target 3.0
        
        sampler2D _ShadowTex;
        float4x4 _ShadowMatrix;
        float _ShadowBias;

        sampler2D _MainTex;
        
        struct Input {
			float2 uv_MainTex;
            float4 shadowCoords;
		};

        void vert(inout appdata_full v, out Input o) {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            float3 worldPos = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1.0)).xyz;
            o.shadowCoords = mul(_ShadowMatrix, float4(worldPos, 1.0)); //What exactly does this mean conceptually? 
        }

		void surf(Input IN, inout SurfaceOutput o)
        {
			//Distance recorded on depth cam texture. Coordinates determined by multiplying the shadow matrix by vertex pos
            float lightDepth = 1.0 - tex2Dproj(_ShadowTex, IN.shadowCoords).r; 

			//If the vertex depth value (z) is less the light depth value determined by looking at the depth texture, the fragment is fully illuminated, otherwise it's darked
            float shadow = (IN.shadowCoords.z - _ShadowBias) < lightDepth ? 1.0 : 0.5;

			//Get expected color from the original texture
			float4 c = tex2D(_MainTex, IN.uv_MainTex);
			o.Albedo = c.rgb * shadow; //multiply color value by shadow value
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
