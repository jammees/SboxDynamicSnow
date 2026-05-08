using Sandbox;
using Sandbox.Diagnostics;
using Snow.Terrain;

namespace Snow.Chunk;

internal sealed partial class SnowTerrainChunk
{
	internal SnowTerrain SnowTerrain;
	internal Sandbox.Terrain Terrain;
	internal Vector2 Id;
	internal int Index;

	private TerrainStorage TerrainStorage => SnowTerrain.Terrain.Storage;

	public SnowTerrainChunk( SnowTerrain terrain, Vector2 id )
	{
		SnowTerrain = terrain;
		Terrain = terrain.Terrain;
		Index = (id.x + id.y * terrain.Division).FloorToInt();
		Id = id;

		CreateBounds( id );
		CreateComputes();
		CreateTextures();
		CreateTerrainCamera();
		SetupRenderlist();
	}

	public void Update()
	{
		Assert.NotNull( RawDeformationMask, "Missing deformation mask!" );
		//Assert.NotNull( SnowMask, "Missing snow mask!" );

		UpdateInternal();
		UpdateDebug();
	}

	public void Destroy()
	{
		ColliderCamera?.DestroyGameObject();

		DisposeTextures();
	}
}
