MODES
{
    Default();
}

COMMON
{
	#include "common/shared.hlsl"
	#include "common/classes/Depth.hlsl"
	#include "Hlsl/Utility.hlsl"
}

CS
{
	// IN
	Texture2D<float> g_tTerrainHeightmap 	< Attribute( "TerrainHeightmap" ); >;	// terrain heightmap
	float g_fSnowHeight 					< Attribute( "SnowHeight" ); >;			// how tall is the snow
	float g_fTerrainHeight					< Attribute( "TerrainHeight" ); >;		// how tall is terrain
	float g_fUvScalar						< Attribute( "UvScalar" ); >;			// scalar of id

	// OUT
	RWTexture2D<float> g_tDeformationMask	< Attribute( "DeformationMask" ); >;	// deformation, no blur

	[numthreads( 8, 8, 1 )]
	void MainCs( uint3 id : SV_DispatchThreadID )
	{
		// not sure how it works but it just works
		float depth = ( Depth::GetWorldPosition(id.xy) - g_vCameraPositionWs ).z;

		// calculate heightmap UV in case texture sizes do not match up
		int heightmapSize = GetFloatTextureDimensions( g_tTerrainHeightmap, 0 ).x;
		float2 heightmapUv = ( (float2)id.xy * g_fUvScalar ) / heightmapSize;

		float terrainHeight = g_tTerrainHeightmap.SampleLevel( g_sPointClamp, heightmapUv, 0 );
		terrainHeight *= g_fTerrainHeight;
		terrainHeight += g_fSnowHeight;

		float deformation = terrainHeight - depth;
		deformation = RemapValClamped( deformation, 0.0, g_fSnowHeight, 1.0, 0.0 );

		g_tDeformationMask[id.xy] = min( g_tDeformationMask[id.xy], deformation );
	}	
}
