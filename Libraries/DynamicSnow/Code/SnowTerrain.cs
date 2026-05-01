using Sandbox;
using Snow.Enums;
using Snow.Terrain;
using System;
using System.Text.Json.Serialization;

namespace Snow;

public sealed partial class SnowTerrain : Component, Component.ExecuteInEditor
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

	/// <summary>
	/// Reference to the terrain component.
	/// </summary>
	[Property]
	[RequireComponent]
	public Sandbox.Terrain Terrain { get; set; }

	private SnowTerrainChunk[] _snowChunks;

	protected override void DrawGizmos()
	{
		float terrainSize = Terrain.Storage.TerrainSize;
		float terrainHeight = Terrain.Storage.TerrainHeight;

		float chunkArea = terrainSize / Division;

		using ( Gizmo.Scope( "SnowTerrain" ) )
		{
			Gizmo.Draw.Color = Color.Green;
			Gizmo.Draw.LineThickness = 2f;

			for ( int x = 0; x < Division; x++ )
			{
				for ( int y = 0; y < Division; y++ )
				{
					Vector3 min = Vector3.Zero;
					min = new Vector3( chunkArea * x, chunkArea * y );

					Vector3 max = Vector3.Zero;
					max = new Vector3( chunkArea * (x + 1), chunkArea * (y + 1), terrainHeight );

					BBox bounds = new BBox( min, max );

					Gizmo.Draw.LineBBox( bounds );

					Gizmo.Draw.Text( $"{x + (y * Division) + 1}", new Transform( bounds.Center ), size: 20f );
				}
			}
		}
	}

	protected override void OnEnabled()
	{
		if ( Game.IsPlaying is false )
			return;

		CreateChunks();
	}

	protected override void OnDisabled()
	{
		if ( Game.IsPlaying is false )
			return;

		Log.Error( "TODO: Dispose chunks!" );
	}

	protected override void OnUpdate()
	{
		Scene.RenderAttributes.SetCombo( "D_DYNAMIC_SNOW_IN_EDITOR", Game.IsPlaying is false );

		if ( Game.IsPlaying is false )
			return;

		foreach ( SnowTerrainChunk chunk in _snowChunks )
		{
			chunk.Update();
		}
	}

	private void CreateChunks()
	{
		_snowChunks = new SnowTerrainChunk[Division * Division];

		for ( int x = 0; x < Division; x++ )
		{
			for ( int y = 0; y < Division; y++ )
			{
				_snowChunks[x + y * Division] = new SnowTerrainChunk(
					this,
					new Vector2( x, y )
				);
			}
		}
	}
}
