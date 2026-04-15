#ifndef TERRAIN_PROCESSING_HLSL
#define TERRAIN_PROCESSING_HLSL

#include "common/shared.hlsl"
#include "common/Bindless.hlsl"
#include "Hlsl/AttributesNames.hlsl"
#include "terrain/TerrainCommon.hlsl"

struct SnowTerrainBuffer
{
    int Mask;
    float SnowHeight;
    float Padding1;
    float Padding2;
};

StructuredBuffer<SnowTerrainBuffer> g_bSnowTerrain < Attribute( TERRAIN_SNOW_BUFFER_ATTR ); >;

class SnowTerrain
{
    static SnowTerrainBuffer Get()
    {
        return g_bSnowTerrain[0];
    }

    static inline float3 ProcessTerrainVertex( float3 worldPosition, float2 uv )
    {
        SnowTerrainBuffer terrainData = Get();
        Texture2D mask = Bindless::GetTexture2D(terrainData.Mask, false);

        float snowHeight = mask.SampleLevel( g_sPointClamp, uv, 0 ).r;

        return worldPosition + float3(0.0, 0.0, snowHeight * terrainData.SnowHeight);
    }

    // modified version of the function inside of terrain/TerrainCommon.hlsl
    static float3 GetTerrainNormal(
        Texture2D heightmap,
        float2 uv,
        float terrainHeight,
        out float3 TangentU,
        out float3 TangentV
    )
    {
        SnowTerrainBuffer terrainData = Get();

        Texture2D snowMask = Bindless::GetTexture2D( terrainData.Mask, false );

        float2 heightmapSize = TextureDimensions2D( heightmap, 0 );
        float2 maskSize = TextureDimensions2D( snowMask, 0 );

        float2 terrainTexelSize = 1.0f / heightmapSize;
        float2 maskTexelSize = 1.0f / maskSize;

        SamplerState sampler = g_sTrilinearBorder;

        float sl = abs( snowMask.SampleLevel( sampler, uv + maskTexelSize * float2( -1, 0 ), 0 ).r );
        float sr = abs( snowMask.SampleLevel( sampler, uv + maskTexelSize * float2( 1, 0 ), 0 ).r );
        float st = abs( snowMask.SampleLevel( sampler, uv + maskTexelSize * float2( 0, -1 ), 0 ).r );
        float sb = abs( snowMask.SampleLevel( sampler, uv + maskTexelSize * float2( 0, 1 ), 0 ).r );

        float l = abs( heightmap.SampleLevel( sampler, uv + terrainTexelSize * float2( -1, 0 ), 0 ).r );
        float r = abs( heightmap.SampleLevel( sampler, uv + terrainTexelSize * float2( 1, 0 ), 0 ).r );
        float t = abs( heightmap.SampleLevel( sampler, uv + terrainTexelSize * float2( 0, -1 ), 0 ).r );
        float b = abs( heightmap.SampleLevel( sampler, uv + terrainTexelSize * float2( 0, 1 ), 0 ).r );

        // Compute dx using central differences
        float dX = l - r;
        float sDX = sl - sr;

        // Compute dy using central differences
        float dY = b - t;
        float sDY = sb - st;

        // Normal strength needs to take in account terrain dimensions rather than just texel scale
        float overallHeight = terrainHeight + terrainData.SnowHeight;
        float normalStrength = overallHeight / Terrain::Get(  ).Resolution;

        float3 normal = normalize( float3( (dX + sDX), (dY + sDY) * -1, 1.0f / normalStrength ) );

        TangentU = normalize( cross( normal, float3( 0, -1, 0 ) ) );
        TangentV = normalize( cross( normal, -TangentU ) );

        return normal;
    }
};

#endif