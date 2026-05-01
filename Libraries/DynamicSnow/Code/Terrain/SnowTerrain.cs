using Sandbox;

namespace Snow.Terrain;

public sealed partial class SnowTerrain : Component, Component.ExecuteInEditor
{
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
