using System.Runtime.InteropServices;

namespace Snow.Buffers;

[StructLayout( LayoutKind.Sequential, Pack = 0 )]
internal struct SnowTerrainBuffer
{
	/// <summary>
	/// The inverse of how large of a 1 dimensional
	/// area does a chunk account for
	/// </summary>
	public float InverseUnitChunkSize;

	/// <summary>
	/// How tall is the snow
	/// </summary>
	public float SnowHeight;

	/// <summary>
	/// Chunk division
	/// </summary>
	public float Division;

	public float TerrainSize;
}
