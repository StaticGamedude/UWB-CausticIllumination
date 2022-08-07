/*
* Handles the shader logic for catpuring the color of specular objects.
*/

#include "UnityCG.cginc"
#include "CommonFunctions.cginc"

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
};

v2f SharedColorVertexShader(
    appdata v,
    float4x4 lightViewProjectionMatrix,
    float3 lightWorldPos,
    float3 lightForwardDirection, 
    int isLightDirectional,
    float specularRefractionIndex,
    float2 uv,
    sampler2D receivingPositionTexture,
    int numOfProjectedVerticies)
{
    v2f o;
	
    float3 worldPos = mul(UNITY_MATRIX_M, v.vertex);
    float3 worldNormal = normalize(mul(transpose(unity_WorldToObject), v.normal));
    float3 estimatedPosition = GetEstimatedSplatPosition(
                                    lightViewProjectionMatrix,
                                    lightWorldPos,
                                    lightForwardDirection,
                                    isLightDirectional,
                                    specularRefractionIndex,
                                    worldPos,
                                    worldNormal,
                                    receivingPositionTexture);

    o.vertex = mul(UNITY_MATRIX_VP, float4(estimatedPosition, 1));
    o.uv = uv;
	
    return o;
}

float4 SharedColorFragmentShader(v2f i, sampler2D specularMainTexture)
{
    // sample the texture
    float4 col = tex2D(specularMainTexture, i.uv);
    return col;
}