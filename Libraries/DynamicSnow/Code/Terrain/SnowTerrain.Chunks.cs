using Sandbox;
using Sandbox.Rendering;
using Snow.Chunk;
using Snow.Utility;

namespace Snow.Terrain;

public sealed partial class SnowTerrain
{
	internal Texture ChunkControlMap;

	[ConCmd("ds_regenerate_control")]
	internal static void RegenerateControlMap()
	{
		Instance.ChunkControlMap?.Dispose();
		Instance.CreateControlMap();
	}

	private void CreateControlMap()
	{
		float terrainSize = Terrain.Storage.TerrainSize;
		int resolution = Terrain.Storage.Resolution;

		ChunkControlMap = Texture.Create( resolution, resolution, ImageFormat.R32_UINT )
			.WithName( $"ChunksControlMap" )
			.WithAnonymous( false )
			.WithGPUOnlyUsage()
			.WithUAVBinding()
			.Finish();

		ComputeShader compute = new( Constants.CREATE_CONTROL_MAP_COMPUTE );

		compute.Attributes.Set( "Division", Division );
		compute.Attributes.Set( "TerrainSize", terrainSize );
		compute.Attributes.Set( "InverseChunkSize", 1f / (terrainSize / Division) );
		compute.Attributes.Set( "ControlMask", ChunkControlMap );

		compute.Dispatch( resolution, resolution, 1 );

		Log.Info( "Created chunk control map" );
	}

	private void CreateChunks()
	{
		_snowChunks = new SnowTerrainChunk[ChunksCount];

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
