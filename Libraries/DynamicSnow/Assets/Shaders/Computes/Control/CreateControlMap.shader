MODES
{
	Default();
}

CS
{
	#include "system.fxc"

	int g_fDivision 					< Attribute( "Division" ); >;
	float g_fTerrainSize				< Attribute( "TerrainSize" ); >;
	float g_fInverseChunkSize			< Attribute( "InverseChunkSize" ); >;

	RWTexture2D<int> g_tControlMask	< Attribute( "ControlMask" ); >;

	float GetControlSize()
	{
		int2 controlDimensions;
		g_tControlMask.GetDimensions( controlDimensions.x, controlDimensions.y );
		return controlDimensions.x;
	}

	float2 GetUv( int2 position )
	{
		// Need to account for position being 1 off
		// the dispatch id goes from 0-1023, dividing by
		// 1024 will never result in 1
		return (float2)position / ( GetControlSize() - 1 );
	}

	int GetChunkId( int2 position )
	{
		float2 uv = GetUv( position );

		if ( uv.x < 0.0 || uv.y < 0.0 || uv.x > 1.0 || uv.y > 1.0 )
			return -1;

		uv *= g_fTerrainSize;

		float2 chunk = uv * g_fInverseChunkSize;
		int idx = (int)floor(chunk.x);
        int idy = (int)floor(chunk.y);
		return idx + idy * g_fDivision;
	}

	[numthreads( 8, 8, 1 )]
	void MainCs( uint3 id : SV_DispatchThreadID )
	{
		int2 position = (int2)id.xy;

		int centerChunk = GetChunkId( position );

		int topChunk = GetChunkId( position + int2( 0, -1 ) );
		int bottomChunk = GetChunkId( position + int2( 0, 1 ) );
		int rightChunk = GetChunkId( position + int2( 1, 0 ) );
		int leftChunk = GetChunkId( position + int2( -1, 0 ) );

		bool isTopValid = topChunk > -1;
		bool isBottomValid = bottomChunk > -1;
		bool isRightValid = rightChunk > -1;
		bool isLeftValid = leftChunk > -1;

		bool isNotAtEdge = isTopValid && isBottomValid && isRightValid && isLeftValid;

		g_tControlMask[ id.xy ] = centerChunk;
	}	
}