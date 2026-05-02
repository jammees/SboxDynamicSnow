using Sandbox;
using Snow.Buffers;

namespace Snow.Terrain;

public sealed partial class SnowTerrain
{
	private GpuBuffer<SnowTerrainBuffer> _terrainBuffer;
	private GpuBuffer<int> _terrainMasksBuffer;

	private void CreateBuffers()
	{
		_terrainBuffer = new( 1, debugName: "Terrain Buffer" );
		_terrainMasksBuffer = new( 64, debugName: "Terrain Masks" );

		Scene.RenderAttributes.Set( "SnowTerrainBuffer", _terrainBuffer );
		Scene.RenderAttributes.Set( "SnowTerrainMasksBuffer", _terrainMasksBuffer );
	}

	private void DisposeBuffers()
	{
		_terrainBuffer?.Dispose();
		_terrainMasksBuffer?.Dispose();
	}
}
