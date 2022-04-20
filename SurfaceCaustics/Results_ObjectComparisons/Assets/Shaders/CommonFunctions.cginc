//Globals set in the CPU
sampler2D _ReceivingPosTexture;
float4x4 _LightViewProjectionMatrix;
float3 _LightWorldPosition;
int _Debug_TransformSpecularGeometry;
int _Debug_EstimationStep;

float CustomDistance(float3 pos1, float3 pos2)
{
    float xDif = pos2.x - pos1.x;
    float yDif = pos2.y - pos1.y;
    float zDif = pos2.z - pos1.z;
    
    return sqrt((xDif * xDif) + (yDif * yDif) + (zDif * zDif));
}

//Get our "splat" position based on a specular vertex's position/normal and the refracted light direction
float3 VertexEstimateIntersection(float3 specularVertexWorldPos, float3 refractedLightRayDirection, sampler2D positionTexture)
{
    float3 p1;
    float4 texPt;
    float2 tc;
    float4 recPos;
    float3 p2;
    
    if (_Debug_EstimationStep == 0) //Inverse the refraction ray direction
    {
        return specularVertexWorldPos + (-1.0 * refractedLightRayDirection);;
    }
    
    p1 = specularVertexWorldPos + (1.0 * refractedLightRayDirection); //P1 - 1 unit along the refracted ray direction from the specular vertex position
    if (_Debug_EstimationStep == 1)
    {
        return p1;
    }
    
    texPt = mul(_LightViewProjectionMatrix, float4(p1, 1));
    tc = 0.5 * (texPt.xy / texPt.w) + float2(0.5, 0.5);
    recPos = tex2Dlod(_ReceivingPosTexture, float4(tc, 1, 1)); //Projected P1 position into the light's space
    if (_Debug_EstimationStep == 2)
    {
        return recPos.xyz;
    }
    
    float newDistance = /*CustomDistance*/distance(specularVertexWorldPos, recPos.xyz); //distance(specularVertexWorldPos, recPos.xzy);
    p2 = specularVertexWorldPos + ( newDistance * refractedLightRayDirection); //P2 - D units Point along the refracted ray direction, where D is the distance from the vertex to the p1 projected position
    if (_Debug_EstimationStep == 3)
    {
        return p2;
    }
    
    texPt = mul(_LightViewProjectionMatrix, float4(p2, 1));
    tc = 0.5 * (texPt.xy / texPt.w) + float2(0.5, 0.5);
    return tex2Dlod(_ReceivingPosTexture, float4(tc, 1, 1)); //Project P2 position into the light's space
}

//Given a specular vertex's position/normal, determine the light refraction direction
float3 RefractRay(float3 specularVertexWorldPos, float3 specularVertexWorldNormal, float refractionIndex)
{
    float3 vertexToLight = normalize(_LightWorldPosition - specularVertexWorldPos);
    float incidentAngle = acos(dot(vertexToLight, specularVertexWorldNormal));
    float refractionAngle = asin(sin(incidentAngle) / refractionIndex);
    float3 refractedRay = -1 * ((vertexToLight / refractionIndex) + ((cos(refractionAngle) - (cos(incidentAngle) / refractionIndex)) * specularVertexWorldNormal));
    return normalize(refractedRay);
}