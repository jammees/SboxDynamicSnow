MODES
{
    Default();
}

COMMON
{
	#include "common/shared.hlsl"
	#include "common/classes/Depth.hlsl"
	#include "Hlsl/Utility.hlsl"
    #include "Hlsl/AttributesNames.hlsl"
}

CS
{
	// IN
	Texture2D<float> g_tTerrainHeightmap 	< Attribute( TERRAIN_HEIGHTMAP_ATTR ); >;	// terrain heightmap
	Texture2D<uint> g_tTerrainControl		< Attribute( TERRAIN_CONTROL_ATTR ); >;
	float g_fSnowHeight 					< Attribute( SNOW_HEIGHT_ATTR ); >;			// how tall is the snow
	float g_fTerrainHeight					< Attribute( TERRAIN_HEIGHT_ATTR ); >;		// how tall is terrain
	float g_fHeightmapUvScaler				< Attribute( HEIGHTMAP_UV_SCALE_ATTR ); >;	// scalar of id

	// OUT
	RWTexture2D<float3> g_tDeformationMask	< Attribute( DEFORMATION_MASK_ATTR ); >;	// deformation, no blur

	[numthreads( 8, 8, 1 )]
	void MainCs( uint3 id : SV_DispatchThreadID )
	{
		// not sure how it works but it just works
		float depth = ( Depth::GetWorldPosition(id.xy) - g_vCameraPositionWs ).z;

		// calculate heightmap UV in case texture sizes do not match up
		int heightmapSize = GetFloatTextureDimensions( g_tTerrainHeightmap, 0 ).x;
		float2 heightmapUv = ( (float2)id.xy * g_fHeightmapUvScaler ) / heightmapSize;

		float terrainHeight = g_tTerrainHeightmap.SampleLevel( g_sBilinearClamp, heightmapUv, 0 );
		terrainHeight *= g_fTerrainHeight;
		terrainHeight += g_fSnowHeight;

		float deformation = terrainHeight - depth;
		deformation = RemapValClamped( deformation, 0.0, g_fSnowHeight, 1.0, 0.0 );

		// need to figure out a way to properly sample the control map of the terrain
		// now each chunk can't just sample it, it needs to somehow map and use the
		// local control texture that occupies in the chunk's boundaries
		// float control = g_tTerrainControl[id.xy * (uint2)g_fHeightmapUvScaler];
		// float isSnow = control > 0 ? 0.0 : 1.0;

		g_tDeformationMask[id.xy] = min( g_tDeformationMask[id.xy], deformation ); //* isSnow;
	}	
}
