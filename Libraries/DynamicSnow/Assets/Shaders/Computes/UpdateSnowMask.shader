MODES
{
    Default();
}

COMMON
{
	#include "common/shared.hlsl"
	#include "Hlsl/AttributesNames.hlsl"
}

CS
{
    // IN
    Texture2D<float> g_tDeformationMask	    < Attribute( DEFORMATION_MASK_ATTR ); >;

	// OUT
    RWTexture2D<float> g_tSnowMask          < Attribute( SNOW_MASK_ATTR ); >;

	[numthreads( 8, 8, 1 )]
	void MainCs( uint3 id : SV_DispatchThreadID )
	{
		g_tSnowMask[id.xy] = g_tDeformationMask[id.xy];
	}	
}
