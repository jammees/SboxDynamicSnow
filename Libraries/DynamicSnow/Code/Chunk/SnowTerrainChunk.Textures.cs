using Sandbox;

namespace Snow.Terrain;

internal sealed partial class SnowTerrainChunk
{
	internal int HighTextureSize => SnowTerrain.HighMaskSize.AsInt();

	private Texture _rawDeformationMask;
	private Texture _snowMask;
	private Texture _renderTarget;

	private void CreateTextures()
	{
		DisposeTextures();

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

		_renderTarget = Texture.CreateRenderTarget()
			.WithMSAA( MultisampleAmount.MultisampleNone )
			.WithSize( HighTextureSize, HighTextureSize )
			.WithFormat( ImageFormat.A8 )
			.Create( name: "SnowColliderRenderTarget", anonymous: false );
	}

	private void DisposeTextures()
	{
		_rawDeformationMask?.Dispose();
		_snowMask?.Dispose();
		_renderTarget?.Dispose();
	}
}
