using Sandbox;

namespace Snow.Terrain;

public sealed partial class SnowTerrain
{
	private Texture _rawDeformationMask;
	private Texture _snowMask;
	private Texture _renderTarget;

	private void CreateTextures( bool disposeOnly = false )
	{
		_rawDeformationMask?.Dispose();
		_snowMask?.Dispose();
		_renderTarget?.Dispose();

		if ( disposeOnly is true )
			return;

		_rawDeformationMask = Texture.Create( HighTextureSize, HighTextureSize, ImageFormat.R16 )
			.WithName( "RawDeformationSnowMask" )
			.WithAnonymous( false )
			.WithGPUOnlyUsage()
			.WithUAVBinding()
			.Finish();

		_snowMask = Texture.Create( HighTextureSize, HighTextureSize, ImageFormat.A8 )
			.WithName( "ProcessedSnowMask" )
			.WithAnonymous( false )
			.WithGPUOnlyUsage()
			.WithUAVBinding()
			.Finish();

		// NOTE: Probably should not use RGBA8888 format here
		// from testing A8 seemed to be fine enough
		_renderTarget = Texture.CreateRenderTarget()
			.WithMSAA( MultisampleAmount.MultisampleNone )
			.WithSize( HighTextureSize, HighTextureSize )
			.Create( name: "SnowColliderRenderTarget", anonymous: false );
	}
}
