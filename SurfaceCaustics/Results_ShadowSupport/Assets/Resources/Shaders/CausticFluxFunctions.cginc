/*
* Hanldes the calcluation of the flux values for each specular vertex. In addition to the flux value, we also store the distance
* from the from the specular vertex position to the splat position to help reduce the number of cameras/textures needed
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
	float flux : TEXCOORD1;
	float distance : TEXCOORD2;
};

/*
* Computes the amount of flux going through a specular vertex. Follows the formula: 
* Flux = (1/V)(Ni * Li) where 
*   V is the number of visible pixels seen by the light source
*   Ni is the vertex normal
*   Li is the incident light vector
* 
* param: lightWorldPos - The world position of the light source
* param: visibleSurfaceArea - The number of pixels occupying the render texture
* param: worldPosition: The specular vertex world position
* param: worldNormal: the specular vertex world normal
* returns: Calculated flux value
*/
float GetFluxContribution(float3 lightWorldPos, float visibleSurfaceArea, float3 worldPosition, float3 worldNormal)
{
    float3 incidentLightVector = normalize(lightWorldPos - worldPosition);

    //Because of the possibility of negative angles between the normal and light vector, we clamp the value
    return (1 / visibleSurfaceArea) * max(dot(worldNormal, incidentLightVector), 0.01);
}

v2f SharedFluxVertexShader(
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
    float3 flux = GetFluxContribution(lightWorldPos, numOfProjectedVerticies, worldPos, worldNormal);

    o.vertex = mul(UNITY_MATRIX_VP, float4(estimatedPosition, 1));
    o.uv = uv;
    o.flux = flux;
    o.distance = distance(worldPos, estimatedPosition);
	
    return o;
}

float4 SharedFluxFragmentShader(v2f i, sampler2D specularMainTexture, float debugFluxMultiplier)
{
    fixed4 col = tex2D(specularMainTexture, i.uv);
    float isVisible = 0;
    float resultingFluxValue = i.flux * debugFluxMultiplier;
    
    if (col.r != 0 || col.g != 0 || col.b != 0)
    {
        isVisible = 1;
    }

    return float4(resultingFluxValue, i.distance, resultingFluxValue, isVisible);
}