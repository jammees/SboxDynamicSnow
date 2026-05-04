#ifndef TERRAIN_PROCESSING_HLSL
#define TERRAIN_PROCESSING_HLSL

#include "common/shared.hlsl"
#include "common/Bindless.hlsl"
#include "Hlsl/AttributesNames.hlsl"
#include "terrain/TerrainCommon.hlsl"

DynamicCombo( D_DYNAMIC_SNOW_IN_EDITOR, 0..1, sys( all ) );
    #define NOT_IN_EDITOR 0

cbuffer SnowTerrainConstantBuffer
{
    float InverseUnitChunkSize; // 1f / terrainSize / division
    float SnowHeight;
    float Division;
    float TerrainSize;
};

StructuredBuffer<int> g_bMaskIndexes < Attribute( TERRAIN_MASKS_BUFFER_ATTR ); >;

class SnowTerrainMasks
{
    static int2 GetChunkId( float2 uv )
    {
        float2 worldUv = uv * TerrainSize;
        float2 chunk = worldUv * InverseUnitChunkSize;

        int idx = (int)floor(chunk.x);
        int idy = (int)floor(chunk.y);

        return int2( idx, idy );
    }

    static int GetChunkIndex( float2 uv )
    {
        int2 chunkId = GetChunkId( uv );
        
        return chunkId.x + chunkId.y * (int)Division;
    }

    static float2 GetLocalUv( float2 globalUv )
    {
        return frac( globalUv * Division );
    }

    static Texture2D Get( float2 uv )
    {
        int index = g_bMaskIndexes[GetChunkIndex( uv )];

        return Bindless::GetTexture2D(index, false);
    }
};

class SnowTerrain
{
    static float3 ProcessTerrainVertex( float3 worldPosition, float2 uv )
    {
        #if ( D_DYNAMIC_SNOW_IN_EDITOR == NOT_IN_EDITOR )
            Texture2D mask = SnowTerrainMasks::Get( uv );
            float2 localUv = SnowTerrainMasks::GetLocalUv( uv );
        
            float snowHeight = mask.SampleLevel( g_sPointClamp, localUv, 0 ).r;
        
            return worldPosition + float3(0.0, 0.0, snowHeight * SnowHeight);
        #else
            return worldPosition;
        #endif
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
        Texture2D snowMask = SnowTerrainMasks::Get( uv );
        float2 localUv = SnowTerrainMasks::GetLocalUv( uv );

        float2 heightmapSize = TextureDimensions2D( heightmap, 0 );
        float2 maskSize = TextureDimensions2D( snowMask, 0 );

        float2 terrainTexelSize = 1.0f / heightmapSize;
        float2 maskTexelSize = 1.0f / maskSize;

        SamplerState sampler = g_sTrilinearBorder;

        float sl = abs( snowMask.SampleLevel( sampler, localUv + maskTexelSize * float2( -1, 0 ), 0 ).r );
        float sr = abs( snowMask.SampleLevel( sampler, localUv + maskTexelSize * float2( 1, 0 ), 0 ).r );
        float st = abs( snowMask.SampleLevel( sampler, localUv + maskTexelSize * float2( 0, -1 ), 0 ).r );
        float sb = abs( snowMask.SampleLevel( sampler, localUv + maskTexelSize * float2( 0, 1 ), 0 ).r );

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
        #if ( D_DYNAMIC_SNOW_IN_EDITOR == NOT_IN_EDITOR )
            float overallHeight = terrainHeight + SnowHeight;
            float normalStrength = overallHeight / Terrain::Get(  ).Resolution;

            float3 normal = normalize( float3( (dX + sDX), (dY + sDY) * -1, 1.0f / normalStrength ) );
        #else
            float normalStrength = terrainHeight / Terrain::Get(  ).Resolution;

            float3 normal = normalize( float3( dX, dY * -1, 1.0f / normalStrength ) );
        #endif

        TangentU = normalize( cross( normal, float3( 0, -1, 0 ) ) );
        TangentV = normalize( cross( normal, -TangentU ) );

        return normal;
    }
};

#endif