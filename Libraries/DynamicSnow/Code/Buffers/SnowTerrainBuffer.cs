using System.Runtime.InteropServices;

namespace Snow.Buffers;

[StructLayout( LayoutKind.Sequential, Pack = 0 )]
internal struct SnowTerrainBuffer
{
	/// <summary>
	/// Index of the snow mask
	/// </summary>
	public int ControlMaskIndex;

	/// <summary>
	/// How tall is the snow
	/// </summary>
	public float SnowHeight;

	public float Padding1;

	public float Padding2;
}
