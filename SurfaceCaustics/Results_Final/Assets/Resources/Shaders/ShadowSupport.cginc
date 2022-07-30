/*
* WARNING: Deprecated
*/

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

float3 ProjectWorldPosToReceiver(float4x4 lightViewProjeciontMatrix, float3 specularVertexWorldPos, sampler2D positionTexture)
{
    float4 texPt = mul(lightViewProjeciontMatrix, float4(specularVertexWorldPos, 1));
    float2 tc = 0.5 * (texPt.xy / texPt.w) + float2(0.5, 0.5);
    return tex2Dlod(positionTexture, float4(tc, 1, 1)).xyz;
}

