
//Get our "splat" position based on a specular vertex's position/normal and the refracted light direction
float3 VertexEstimateIntersection(
    float4x4 lightViewProjectMatrix,
    float3 specularVertexWorldPos, 
    float3 refractedLightRayDirection, 
    sampler2D positionTexture)
{
    float3 p1 = specularVertexWorldPos + (1.0 * refractedLightRayDirection); //P1 - 1 unit along the refracted ray direction from the specular vertex position
    float4 texPt = mul(lightViewProjectMatrix, float4(p1, 1));
    float2 tc = 0.5 * (texPt.xy / texPt.w) + float2(0.5, 0.5);
    float4 recPos = tex2Dlod(positionTexture, float4(tc, 1, 1)); //Projected P1 position into the light's space;
    float newDistance = distance(specularVertexWorldPos, recPos.xyz);
    float3 p2 = specularVertexWorldPos + (newDistance * refractedLightRayDirection); //P2 - D units Point along the refracted ray direction, where D is the distance from the vertex to the p1 projected position

    texPt = mul(lightViewProjectMatrix, float4(p2, 1));
    tc = 0.5 * (texPt.xy / texPt.w) + float2(0.5, 0.5);
    return tex2Dlod(positionTexture, float4(tc, 1, 1)); //Project P2 position into the light's space
}

//Given a specular vertex's position/normal, determine the light refraction direction
float3 RefractRay(float3 lightPosition, float3 specularVertexWorldPos, float3 specularVertexWorldNormal, float refractionIndex)
{
    float3 vertexToLight = normalize(lightPosition - specularVertexWorldPos);
    float incidentAngle = acos(dot(vertexToLight, specularVertexWorldNormal));
    float refractionAngle = asin(sin(incidentAngle) / refractionIndex);
    float3 refractedRay = -1 * ((vertexToLight / refractionIndex) + ((cos(refractionAngle) - (cos(incidentAngle) / refractionIndex)) * specularVertexWorldNormal));
    return normalize(refractedRay);
}

float3 GetEstimatedSplatPosition(
    float4x4 lightViewProjectMatrix,
    float3 lightPosition,
    float sepcularObjectRefractionIndex,
    float3 specularWorldPos,
    float3 specularWorldNormal,
    sampler2D receiverPositionTexture
)
{
    float3 refractedDirection = RefractRay(lightPosition, specularWorldPos, specularWorldNormal, sepcularObjectRefractionIndex);
    return VertexEstimateIntersection(lightViewProjectMatrix, specularWorldPos, refractedDirection, receiverPositionTexture);
}