#include "UnityCG.cginc"
#include "CommonFunctions.cginc"

float _DebugFlux;

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
 */
float2 GetCoordinatesForSpecularTexture(float4x4 lightViewProjectionMatrix, float3 worldPos)
{
    float4 texPt = mul(lightViewProjectionMatrix, float4(worldPos, 1));
    float2 tc = 0.5 * texPt.xy / texPt.w + float2(0.5, 0.5);
    return tc;
}

float GetFlux(float4x4 lightViewProjectionMatrix, sampler2D sourceTexture, float3 worldPos)
{
    float2 tc = GetCoordinatesForSpecularTexture(lightViewProjectionMatrix, worldPos);
    float4 fluxVals = tex2D(sourceTexture, tc);
    return fluxVals.x;
}

float GetDistance(float4x4 lightViewProjectionMatrix, sampler2D sourceTexture, float3 worldPos)
{
    float2 tc = GetCoordinatesForSpecularTexture(lightViewProjectionMatrix, worldPos);
    float4 distanceVals = tex2D(sourceTexture, tc);
    return distanceVals.y;
}

fixed4 GetCausticColor(float4x4 lightViewProjectionMatrix, sampler2D sourceTexture, float3 worldPos)
{
    float2 tc = GetCoordinatesForSpecularTexture(lightViewProjectionMatrix, worldPos);
    fixed4 causticColor = tex2D(sourceTexture, tc);
    return causticColor;
}

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
    float specularRefractionIndex,
    float2 uv,
    sampler2D receivingPositionTexture)
{
    v2f o;
    float3 worldPos = mul(UNITY_MATRIX_M, v.vertex);
    float3 worldNormal = normalize(mul(transpose(unity_WorldToObject), v.normal));
    float3 refractedDirection = RefractRay(lightWorldPos, worldPos, worldNormal, specularRefractionIndex);
    float3 estimatedPosition = GetEstimatedSplatPosition(
                                            lightViewProjectionMatrix,
                                            lightWorldPos,
                                            specularRefractionIndex,
                                            worldPos,
                                            worldNormal,
                                            receivingPositionTexture);

    o.vertex = mul(UNITY_MATRIX_VP, float4(estimatedPosition, 1));
    o.uv = uv;
    o.splatPos = estimatedPosition;
    
    return o;
}

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
    float finalIntensity = flux * exp((-specularAbsorbtionCoefficient * d));
    fixed4 causticColor = GetCausticColor(lightViewProjectionMatrix, causticColorTexture, i.splatPos) * (ClampSpecularColorFactor(specularColorFactor));
    
    return finalIntensity * lightColor * causticColor * lightIntensity;
}

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