MODES
{
    Default();
}

COMMON
{
    #include "Hlsl/AttributesNames.hlsl"
}

CS
{
    // IN OUT
	RWTexture2D<float> g_tDeformationMask < Attribute( DEFORMATION_MASK_ATTR ); >;

	[numthreads( 8, 8, 1 )]
	void MainCs( uint3 id : SV_DispatchThreadID )
	{
		g_tDeformationMask[id.xy] = 1.0;
	}	
}
