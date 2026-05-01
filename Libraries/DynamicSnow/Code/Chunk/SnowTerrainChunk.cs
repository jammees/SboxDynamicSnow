using Sandbox;

namespace Snow.Terrain;

internal sealed partial class SnowTerrainChunk
{
	internal SnowTerrain SnowTerrain;

	private TerrainStorage TerrainStorage => SnowTerrain.Terrain.Storage;

	public SnowTerrainChunk( SnowTerrain terrain, Vector2 id )
	{
		SnowTerrain = terrain;

		CreateBounds( id );
		CreateTerrainCamera();
	}

	~SnowTerrainChunk()
	{
	}
}
