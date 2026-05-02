using Sandbox;
using Snow.Enums;
using System;
using System.Text.Json.Serialization;

namespace Snow.Terrain;

public sealed partial class SnowTerrain
{
	/// <summary>
	/// How high should the snow be. Recommended to keep this
	/// relatively low to prevent the player camera clipping into
	/// the rendered mesh when in first-person.
	/// </summary>
	[Property]
	[Group( "Rendering" )]
	[Range( 0f, 100f ), Step( 0.1f )]
	public float SnowHeight
	{
		get => field;
		set
		{
			field = MathF.Max( value, 0f );

			if ( _snowChunks is null )
				return;

			foreach ( SnowTerrainChunk chunk in _snowChunks )
			{
				chunk.UpdateTerrainCameraPosition();
			}
		}
	}

	/// <summary>
	/// Objects with these tags can collide with the snow and
	/// deform it.
	/// </summary>
	[Property]
	[Group( "Rendering" )]
	public TagSet SnowColliderTags
	{
		get => field;
		set
		{
			field = value;

			if ( _snowChunks is null )
				return;

			foreach ( SnowTerrainChunk chunk in _snowChunks )
			{
				chunk.ColliderCamera.RenderTags = field;
			}
		}
	}

	/// <summary>
	/// How big should the working texture be for the following: render target, deformation mask
	/// and the snow mask. This results in textures scaling exponentially. A 1024x1024 option
	/// takes ~17 MBs of memory.
	/// </summary>
	[Property]
	[Group( "Rendering" )]
	public SupportedTextureSizes HighMaskSize { get; set; } = SupportedTextureSizes.Medium;

	/// <summary>
	/// How many chunks should be used? For example, a divison of 1 means
	/// a single chunk, but a division 2 will result in 4, then 9 and so on.
	/// To get how many chunks you'll have, simply square the division.
	/// </summary>
	[Property]
	[Group( "Chunking" )]
	[Range( 1f, 10f ), Step( 1f )]
	public int Division { get; set; } = 1;

	[Property]
	[Group( "Chunking" )]
	[ReadOnly, JsonIgnore]
	public int ChunksCount => Division * Division;
}
