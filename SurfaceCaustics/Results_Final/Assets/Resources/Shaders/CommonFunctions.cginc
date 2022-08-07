
float _DrewDistanceTest;

/*
* Defines the "common" functions used by most shaders in the applications. Helps with determining the refraction ray direction
* and the vertex estimated (splat) positions.
*/

/*
* Get our "splat" position based on a specular vertex's position/normal and the refracted light direction
* param: lightViewProjectionMatrix - A light source's view projection matrix to help transform a point into the light's space
* param: specularVertexWorldPos - A specular vertex position in world space
* param: refractedLightRayDirection - The direction of the refracted light ray
* param: positionTexture - The texture containing the receiving object world positions
* return: Cacluated splat position
*/
float3 VertexEstimateIntersection(
    float4x4 lightViewProjectMatrix,
    float3 specularVertexWorldPos, 
    float3 refractedLightRayDirection, 
    sampler2D positionTexture)
{
    float3 p1 = specularVertexWorldPos + (_DrewDistanceTest * refractedLightRayDirection); //P1 - 1 unit along the refracted ray direction from the specular vertex position
    float4 texPt = mul(lightViewProjectMatrix, float4(p1, 1));
    float2 tc = 0.5 * (texPt.xy / texPt.w) + float2(0.5, 0.5);
    float4 recPos = tex2Dlod(positionTexture, float4(tc, 1, 1)); //Projected P1 position into the light's space;
    float newDistance = distance(specularVertexWorldPos, recPos.xyz);
    float3 p2 = specularVertexWorldPos + (newDistance * refractedLightRayDirection); //P2 - D units Point along the refracted ray direction, where D is the distance from the vertex to the p1 projected position

    texPt = mul(lightViewProjectMatrix, float4(p2, 1));
    tc = 0.5 * (texPt.xy / texPt.w) + float2(0.5, 0.5);
    return tex2Dlod(positionTexture, float4(tc, 1, 1)); //Project P2 position into the light's space
}

/*
* Given a specular vertex's position/normal, determine the light refraction direction
* param: lightPosition - The world position of a light source
* param: lightForwardDirection - The direction of the light source
* param: isLightDirectional - A flag indicating whether the light source is a directional light (1 for yes, 0 otherwise)
* param: specularVertexWorldPos - The specular vertex world position 
* param: specularVertexWorldNormal - The specular vertex world normal
* param: refractionIndex - The refraction index of the specular object
*/
float3 RefractRay(float3 lightPosition, float3 lightForwardDirection, int isLightDirectional, float3 specularVertexWorldPos, float3 specularVertexWorldNormal, float refractionIndex)
{
    float3 vertexToLight = normalize(lightPosition - specularVertexWorldPos);
    
    // If we're supporting a directional light, the vertex to light direction should always be the the light camera's forward direction.
    // Essentially treating as if all the light rays are parallel
    if (isLightDirectional == 1)
    {
        vertexToLight = -lightForwardDirection;
    }
    
    float angleBetween = dot(vertexToLight, specularVertexWorldNormal);
    if (angleBetween < 0)
    {
        return 0;
    }
    
    float incidentAngle = acos(angleBetween);
    float refractionAngle = asin(sin(incidentAngle) / refractionIndex);
    float3 refractedRay = -1 * ((vertexToLight / refractionIndex) + ((cos(refractionAngle) - (cos(incidentAngle) / refractionIndex)) * specularVertexWorldNormal));

    return normalize(refractedRay);
}

/*
* Converts a vertex position to a splat position.
* param: lightViewProjectMatrix - The light source's view projection matrix
* param: lightPosition - The world position of a light source
* param: lightForwardDirection - The direction of the light source
* param: isLightDirectional - A flag indicating whether the light source is a directional light (1 for yes, 0 otherwise)
* param: specularObjectRefractionIndex - The refraction index of the specular object
* param: specularWorldPos - The specular vertex world position
* param: specularWorldNormal - The specular vertex world normal
* param: receiverPositionTexture - The texture containing the receiving object world positions
*/
float3 GetEstimatedSplatPosition(
    float4x4 lightViewProjectMatrix,
    float3 lightPosition,
    float3 lightForwardDirection, 
    int isLightDirectional,
    float sepcularObjectRefractionIndex,
    float3 specularWorldPos,
    float3 specularWorldNormal,
    sampler2D receiverPositionTexture
)
{
    float3 refractedDirection = RefractRay(lightPosition, lightForwardDirection, isLightDirectional, specularWorldPos, specularWorldNormal, sepcularObjectRefractionIndex);
    return VertexEstimateIntersection(lightViewProjectMatrix, specularWorldPos, refractedDirection, receiverPositionTexture);
}