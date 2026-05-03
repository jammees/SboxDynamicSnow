using Sandbox;
using Snow.Buffers;
using Snow.Utility;
using System.Collections.Generic;

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

					Gizmo.Draw.Text( $"{x + (y * Division) + 1}", new Transform( bounds.Center ), size: 20f );
				}
			}
		}
	}

	protected override void OnEnabled()
	{
		if ( Game.IsPlaying is false )
			return;

		ChunkMasks = new int[ChunksCount];

		CreateBuffers();
		CreateChunks();

		// for now just upload it once
		// however, once high and low res masks are in the picture
		// we'll need to update the buffer if required
		_terrainMasksBuffer.SetData( ChunkMasks );

		// same here, should only need to update it once
		// snow height changes
		// how many chunks or how large the terrain is realistically
		// shouldn't change mid-game
		UploadToGPU();
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

		_terrainBuffer.SetData( new List<SnowTerrainBuffer>() { bufferData } );
	}
}
