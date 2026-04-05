using System.Runtime.InteropServices;

namespace Snow.Terrain;

[StructLayout( LayoutKind.Sequential, Pack = 0 )]
internal struct SnowTerrainBuffer
{
	/// <summary>
	/// Where is the observer's camera relative to the terrain,
	/// range from 0 to 1
	/// </summary>
	public Vector2 PlayerCamera;

	/// <summary>
	/// Index of the snow mask that is rendered at a
	/// lower resolution
	/// </summary>
	public int MaskLow;

	/// <summary>
	/// Index of the snow mask that is rendered at a
	/// higher resolution near the player
	/// </summary>
	public int MaskHigh;

	/// <summary>
	/// How tall is the snow
	/// </summary>
	public float SnowHeight;

	public float BlendStartDistance;

	public float HighRenderDistance;
}
