MODES
{
    Default();
}

COMMON
{
	#include "common.fxc"
	#include "common/shared.hlsl"
	#include "common/classes/Depth.hlsl"
}

CS
{
	DynamicCombo( D_PIPELINE_STATE, 0..2, Sys( ALL ) );

	// IN
	Texture2D<float> g_tTerrainHeightmap 	< Attribute( "TerrainHeightmap" ); >;	// terrain heightmap
	float g_fSnowHeight 					< Attribute( "SnowHeight" ); >;			// how tall is the snow
	float g_fTerrainHeight					< Attribute( "TerrainHeight" ); >;		// how tall is terrain

	// OUT
	RWTexture2D<float> g_tSnowMask 			< Attribute( "SnowMask" ); >;			// deformation, no blur

	void PipielineSetupMask( uint3 id: SV_DispatchThreadID )
	{
		g_tSnowMask[id.xy] = 1.0;
	}

	void PipelineUpdateMask( uint3 id: SV_DispatchThreadID )
	{
		// not sure how it works but it just works
		float depth = (Depth::GetWorldPosition(id.xy) - g_vCameraPositionWs).z;

		float terrainHeight = g_tTerrainHeightmap[id.xy];
		terrainHeight *= g_fTerrainHeight;
		terrainHeight += g_fSnowHeight;

		float deformation = terrainHeight - depth;
		deformation = RemapValClamped( deformation, 0.0, g_fSnowHeight, 1.0, 0.0 );

		g_tSnowMask[id.xy] = min( g_tSnowMask[id.xy], deformation );
	}

	[numthreads( 8, 8, 1 )]
	void MainCs( uint3 id : SV_DispatchThreadID )
	{
		#if ( D_PIPELINE_STATE == 0 ) // SETUP
			PipielineSetupMask( id );
		#elif ( D_PIPELINE_STATE == 1 ) // UPDATE
			PipelineUpdateMask( id );
		#elif ( D_PIPELINE_STATE == 2 ) // BLUR

		#endif
	}	
}
