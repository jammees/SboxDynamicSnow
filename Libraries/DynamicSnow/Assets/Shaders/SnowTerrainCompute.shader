MODES
{
    Default();
}

CS
{
	#include "common.fxc"

	// TODO: Remember to account for MSAA depth!
	Texture2DMS<float> g_tRawMask < Attribute( "Depth" ); >;
	RWTexture2D<float> g_tSnowMask < Attribute( "SnowMask" ); >;

	[numthreads( 8, 8, 1 )]
	void MainCs( uint3 id : SV_DispatchThreadID )
	{
		g_tSnowMask[id.xy] = g_tRawMask.Load(id.xy, 0); //g_tRawMask[id.xy];
	}	
}