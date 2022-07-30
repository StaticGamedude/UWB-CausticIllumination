/*
* WARNING: Deprecated
*/

#include "CommonFunctions.cginc"

float _DebugFluxMultiplier;

float GetFluxContribution(float3 lightWorldPos, float visibleSurfaceArea, float3 worldPosition, float3 worldNormal)
{
    float3 incidentLightVector = normalize(lightWorldPos - worldPosition);

    //Because of the possibility of negative angles between the normal and light vector, we clamp the value
    return (1 / visibleSurfaceArea) * max(dot(worldNormal, incidentLightVector), 0.01) * _DebugFluxMultiplier;
}