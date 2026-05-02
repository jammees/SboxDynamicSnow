namespace Snow.Terrain;

public sealed partial class SnowTerrain
{
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
