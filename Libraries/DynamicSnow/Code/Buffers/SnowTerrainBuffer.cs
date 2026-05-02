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

	public float Padding1;

	public float Padding2;
}
