using Sandbox;
using Sandbox.Diagnostics;
using Sandbox.Rendering;

namespace Snow.Terrain;

internal sealed partial class SnowTerrainChunk
{
	internal SnowTerrain SnowTerrain;
	internal Sandbox.Terrain Terrain;
	internal Vector2 Id;

	private TerrainStorage TerrainStorage => SnowTerrain.Terrain.Storage;

	public SnowTerrainChunk( SnowTerrain terrain, Vector2 id )
	{
		SnowTerrain = terrain;
		Terrain = terrain.Terrain;
		Id = id;

		CreateBounds( id );
		CreateComputes();
		CreateTextures();
		CreateTerrainCamera();

		_isMaskCleared = false;

		_terrainBuffer = new( 1, GpuBuffer.UsageFlags.Structured, "SnowTerrainBuffer" );

		_renderList = new();
		ColliderCamera.AddCommandList( _renderList, Stage.AfterDepthPrepass, 1000 );
	}

	~SnowTerrainChunk()
	{
	}

	public void Update()
	{
		Assert.NotNull( _rawDeformationMask, "Missing deformation mask!" );
		Assert.NotNull( _snowMask, "Missing snow mask!" );

		UpdateTerrain();
	}
}
