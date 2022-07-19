/*
* Defines the shader logic for computing the final caustic color/result.
*/
#include "UnityCG.cginc"
#include "CommonFunctions.cginc"

float _DebugFlux;
int _CausticBlurKernalSize;

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
	float3 splatPos : TEXCOORD1;
};

/*
* Given the world position of the receiving object, get the texture
* coordinates that can be used to map into a caustic texture.
* param: lightViewProjectionMatrix - The light camera's view projection matrix
* param: worldPos - The vertex world position
* returns: Texture coordinates used to index into a caustic texture
*/
float2 GetCoordinatesForSpecularTexture(float4x4 lightViewProjectionMatrix, float3 worldPos)
{
    float4 texPt = mul(lightViewProjectionMatrix, float4(worldPos, 1));
    float2 tc = 0.5 * texPt.xy / texPt.w + float2(0.5, 0.5);
    return tc;
}

/*
* Get the flux value from a texture for a light source
* param: lightViewProjectionMatrix - The light camera's view projection matrix
* param: sourceTexture - The texture containing the flux data
* param: worldPos - The vertex world position
* returns: Flux value read from the flux texture
*/
float GetFlux(float4x4 lightViewProjectionMatrix, sampler2D sourceTexture, float3 worldPos)
{
    float2 tc = GetCoordinatesForSpecularTexture(lightViewProjectionMatrix, worldPos);
    float4 fluxVals = tex2D(sourceTexture, tc);
    return fluxVals.x;
}

/*
* Get the distance value from a texture for a light source. The distance is the distance from the specular vertex
* to the splat position
* param: lightViewProjectionMatrix - The light camera's view projection matrix
* param: sourceTexture - The texture containing the flux data
* param: worldPos - The vertex world position
* returns: Distance value read from the flux texture
*/
float GetDistance(float4x4 lightViewProjectionMatrix, sampler2D sourceTexture, float3 worldPos)
{
    float2 tc = GetCoordinatesForSpecularTexture(lightViewProjectionMatrix, worldPos);
    float4 distanceVals = tex2D(sourceTexture, tc);
    return distanceVals.y;
}

/*
* Determines if the specular vertex position is visible by the caustic camera
* param: lightViewProjectionMatrix - The light camera's view projection matrix
* param: sourceTexture - The texture containing the flux data
* param: worldPos - The vertex world position
* returns: 1 if the point is visible. 0 otherwise
*/
float GetIsVisible(float4x4 lightViewProjectionMatrix, sampler2D sourceTexture, float3 worldPos)
{
    float2 tc = GetCoordinatesForSpecularTexture(lightViewProjectionMatrix, worldPos);
    float4 visibleValues = tex2D(sourceTexture, tc);
    return visibleValues.w > 0 ? 1 : 0;
}

/*
* Gets the caustic color for a specular vertex
* param: lightViewProjectionMatrix - The light camera's view projection matrix
* param: sourceTexture - The texture containing the flux data
* param: worldPos - The vertex world position
* returns: The causic color found in the color texture for a specular vertex
*/
fixed4 GetCausticColor(float4x4 lightViewProjectionMatrix, sampler2D sourceTexture, float3 worldPos)
{
    float2 tc = GetCoordinatesForSpecularTexture(lightViewProjectionMatrix, worldPos);
    fixed4 causticColor = tex2D(sourceTexture, tc);
    return causticColor;
}

/*
* Utility method for making sure our color factor is between 0 and 1
* param: userFactorValue - Desired factor value
* returns: Valid factor range between 0 and 1
*/
float ClampSpecularColorFactor(float userFactorValue)
{
    if (userFactorValue > 1)
    {
        return 1;
    }
    else if (userFactorValue <= 0)
    {
        return 0.0001;
    }
    
    return userFactorValue;
}

v2f
    SharedCausticFinalVertexShader(
    appdata v,
    float4x4 lightViewProjectionMatrix,
    float3 lightWorldPos,
    float3 lightForwardDirection,
    int isLightDirectional,
    float specularRefractionIndex,
    float2 uv,
    sampler2D receivingPositionTexture)
{
    v2f o;
    float3 worldPos = mul(UNITY_MATRIX_M, v.vertex);
    float3 worldNormal = normalize(mul(transpose(unity_WorldToObject), v.normal));
    //float3 refractedDirection = RefractRay(lightWorldPos, worldPos, worldNormal, specularRefractionIndex);
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
    o.splatPos = estimatedPosition;
    
    return o;
}

/*
* Shared vertex shader which "transforms" the geometry to the receiving object's geometry. The final color is computed by the following formula:
* finalcolor = I * e^(-kd) where
*   I is the incident light intensity
*   K is the absorption coefficient
*   d is the distance that light travles through the specular object
*/
fixed4 SharedCausticFinalFragmentShader(
    v2f i, 
    float4x4 lightViewProjectionMatrix, 
    float3 lightWorldPos,
    fixed4 lightColor,
    float lightIntensity, 
    sampler2D fluxDataTexture, 
    sampler2D causticColorTexture, 
    float specularAbsorbtionCoefficient,
    float specularColorFactor)
{
    float flux = GetFlux(lightViewProjectionMatrix, fluxDataTexture, i.splatPos);
    float d = GetDistance(lightViewProjectionMatrix, fluxDataTexture, i.splatPos);
    float isVisibile = GetIsVisible(lightViewProjectionMatrix, fluxDataTexture, i.splatPos);
    float finalIntensity = flux * exp((-specularAbsorbtionCoefficient * d));
    fixed4 causticColor = GetCausticColor(lightViewProjectionMatrix, causticColorTexture, i.splatPos) * (ClampSpecularColorFactor(specularColorFactor));
    fixed4 computedColorValue = finalIntensity * lightColor * causticColor * lightIntensity;

    return computedColorValue;
}

/*
* Shared fragment shader when computing shadows. This fragment shader is essentially a simplied version of the shader above.
* TODO: We can probably reduce the duplicate code with the fragment shader above (SharedCausticFinalFragmentShader)
*/
fixed4 SharedShadowFragmentShader(
    v2f i,
    float4x4 lightViewProjectionMatrix,
    float3 lightWorldPos,
    fixed4 lightColor,
    float lightIntensity,
    sampler2D fluxDataTexture,
    float specularAbsorbtionCoefficient,
    float specularColorFactor)
{
    float flux = GetFlux(lightViewProjectionMatrix, fluxDataTexture, i.splatPos);
    float d = GetDistance(lightViewProjectionMatrix, fluxDataTexture, i.splatPos);
    float finalIntensity = flux * exp((-specularAbsorbtionCoefficient * d));
    
    return finalIntensity * lightColor * lightIntensity;
}