using Sandbox;
using Sandbox.Rendering;
using Snow.Buffers;
using Snow.Chunk;
using Snow.Utility;

namespace Snow.Terrain;

public sealed partial class SnowTerrain : Component, Component.ExecuteInEditor
{
	/// <summary>
	/// Reference to the terrain component.
	/// </summary>
	[Property]
	[RequireComponent]
	public Sandbox.Terrain Terrain { get; set; }

	internal int[] ChunkMasks;

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

					Gizmo.Draw.Text( $"{x + y * Division}", new Transform( bounds.Center ), size: 20f );
				}
			}
		}
	}

	protected override void OnEnabled()
	{
		if ( Game.IsPlaying is false )
			return;

		ChunkMasks = new int[ChunksCount];

		CreateChunks();
		CreateBuffers();
	}

	protected override void OnDisabled()
	{
		if ( Game.IsPlaying is false )
			return;

		DisposeBuffers();

		foreach ( SnowTerrainChunk chunk in _snowChunks )
		{
			chunk.Destroy();
		}
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

		UploadToGPU();
		DrawDebug();
	}

	private void DrawDebug()
	{
		DebugSystem debug = DebugSystem.Current;
		debug.Add( "Masks:" );
		for ( int i = 0; i < ChunkMasks.Length; i++ )
		{
			debug.Add( $"     [{i}] {ChunkMasks[i]}" );
		}
		debug.Add( $"Chunks: {Division * Division}" );
		debug.Add( $"Inverse Unit Chunk: {1f / (Terrain.Storage.TerrainSize / Division)}" );
	}

	private void UploadToGPU()
	{
		SnowTerrainBuffer bufferData = new()
		{
			InverseUnitChunkSize = 1f / (Terrain.Storage.TerrainSize / Division),
			SnowHeight = SnowHeight,
			Division = Division,
			TerrainSize = Terrain.Storage.TerrainSize,
		};

		Scene.RenderAttributes.SetData( "SnowTerrainConstantBuffer", bufferData );

		Scene.RenderAttributes.Set( "SnowTerrainMasksBuffer", _terrainMasksBuffer );
	}
}
