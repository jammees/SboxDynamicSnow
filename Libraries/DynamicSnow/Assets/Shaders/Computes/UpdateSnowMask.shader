MODES
{
    Default();
}

COMMON
{
	#include "common/shared.hlsl"
}

CS
{
    // IN
    Texture2D<float> g_tDeformationMask	    < Attribute( "DeformationMask" ); >;

	// OUT
    RWTexture2D<float> g_tSnowMask          < Attribute( "SnowMask" ); >;

	[numthreads( 8, 8, 1 )]
	void MainCs( uint3 id : SV_DispatchThreadID )
	{
		g_tSnowMask[id.xy] = g_tDeformationMask[id.xy];
	}	
}
