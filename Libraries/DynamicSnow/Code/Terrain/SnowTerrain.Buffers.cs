using Sandbox;

namespace Snow.Terrain;

public sealed partial class SnowTerrain
{
	private GpuBuffer<int> _terrainMasksBuffer;

	private void CreateBuffers()
	{
		_terrainMasksBuffer = new( 64, debugName: "Terrain Masks" );
	}

	private void DisposeBuffers()
	{
		_terrainMasksBuffer?.Dispose();
	}
}
