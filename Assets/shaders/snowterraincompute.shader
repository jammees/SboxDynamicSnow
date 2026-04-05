MODES
{
    Default();
}

CS
{
	#include "system.fxc"

	Texture2D<float> g_tRawMask < Attribute( "RawMask" ); >;
	RWTexture2D<float> g_tSnowMask < Attribute( "SnowMask" ); >;

	[numthreads( 8, 8, 1 )]
	void MainCs( uint3 id : SV_DispatchThreadID )
	{
		g_tSnowMask[id.xy] = 1.0;
	}	
}