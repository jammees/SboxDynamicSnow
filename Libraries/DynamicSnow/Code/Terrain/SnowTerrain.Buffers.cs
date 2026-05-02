using Sandbox;
using Snow.Buffers;

namespace Snow.Terrain;

public sealed partial class SnowTerrain
{
	private GpuBuffer<SnowTerrainBuffer> _terrainBuffer;
	private GpuBuffer<int> _terrainMasks;

	private void CreateBuffers()
	{
		_terrainBuffer = new( 1, debugName: "Terrain Buffer" );
		_terrainMasks = new( 64, debugName: "Terrain Masks" );

		Scene.RenderAttributes.Set( "SnowTerrainBuffer", _terrainBuffer );
		Scene.RenderAttributes.Set( "SnowTerrainMasksBuffer", _terrainMasks );
	}

	private void DisposeBuffers()
	{
		_terrainBuffer?.Dispose();
		_terrainMasks?.Dispose();
	}
}
