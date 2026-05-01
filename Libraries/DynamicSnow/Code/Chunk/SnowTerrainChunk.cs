using Sandbox;

namespace Snow.Terrain;

internal sealed partial class SnowTerrainChunk
{
	internal SnowTerrain SnowTerrain;
	internal Vector2 Id;

	private TerrainStorage TerrainStorage => SnowTerrain.Terrain.Storage;

	public SnowTerrainChunk( SnowTerrain terrain, Vector2 id )
	{
		SnowTerrain = terrain;
		Id = id;

		CreateBounds( id );
		CreateTerrainCamera();
	}

	~SnowTerrainChunk()
	{
	}
}
