#ifndef TERRAIN_PROCESSING_HLSL
#define TERRAIN_PROCESSING_HLSL

#include "common/Bindless.hlsl"

struct SnowTerrainBuffer
{
    int Mask;
    float SnowHeight;
    float Padding1;
    float Padding2;
};

StructuredBuffer<SnowTerrainBuffer> g_bSnowTerrain < Attribute("SnowTerrainBuffer"); >;

class SnowTerrain
{
    static inline float3 ProcessTerrainVertex( float3 worldPosition, float2 uv )
    {
        SnowTerrainBuffer terrainData = g_bSnowTerrain[0];
        Texture2D mask = Bindless::GetTexture2D(terrainData.Mask, false);

        float snowHeight = mask.SampleLevel( g_sPointClamp, uv, 0 ).r;

        return worldPosition + float3(0.0, 0.0, snowHeight * terrainData.SnowHeight);
    }
};

#endif