using Sandbox;

namespace Snow.Terrain;

internal sealed partial class SnowTerrainChunk
{
	internal int HighTextureSize => SnowTerrain.HighMaskSize.AsInt();

	internal Texture RawDeformationMask;
	internal Texture SnowMask;
	internal Texture RenderTarget;

	private void CreateTextures()
	{
		DisposeTextures();

		RawDeformationMask = Texture.Create( HighTextureSize, HighTextureSize, ImageFormat.R16 )
			.WithName( $"RawDeformationSnowMask{Id}" )
			.WithAnonymous( false )
			.WithGPUOnlyUsage()
			.WithUAVBinding()
			.Finish();

		SnowMask = Texture.Create( HighTextureSize, HighTextureSize, ImageFormat.A8 )
			.WithName( $"ProcessedSnowMask{Id}" )
			.WithAnonymous( false )
			.WithGPUOnlyUsage()
			.WithUAVBinding()
			.Finish();

		RenderTarget = Texture.CreateRenderTarget()
			.WithMSAA( MultisampleAmount.MultisampleNone )
			.WithSize( HighTextureSize, HighTextureSize )
			.WithFormat( ImageFormat.A8 )
			.Create( name: $"SnowColliderRenderTarget{Id}", anonymous: false );
	}

	private void DisposeTextures()
	{
		RawDeformationMask?.Dispose();
		SnowMask?.Dispose();
		RenderTarget?.Dispose();
	}
}
