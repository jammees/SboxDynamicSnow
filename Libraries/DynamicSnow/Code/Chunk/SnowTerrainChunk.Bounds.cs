namespace Snow.Terrain;

internal sealed partial class SnowTerrainChunk
{
	internal BBox Bounds;

	private void CreateBounds( Vector2 id )
	{
		float terrainSize = TerrainStorage.TerrainSize;
		float terrainHeight = TerrainStorage.TerrainHeight;

		float chunkArea = terrainSize / SnowTerrain.Division;

		Vector3 min = Vector3.Zero;
		min = new Vector3( chunkArea * id.x, chunkArea * id.y );

		Vector3 max = Vector3.Zero;
		max = new Vector3( chunkArea * (id.x + 1), chunkArea * (id.y + 1), terrainHeight );

		Bounds = new BBox( min, max );
	}
}
