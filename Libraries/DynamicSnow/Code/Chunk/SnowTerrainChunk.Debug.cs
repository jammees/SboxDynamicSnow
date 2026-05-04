using Sandbox;
using Snow.Terrain;

namespace Snow.Chunk;

internal sealed partial class SnowTerrainChunk
{
	[ConVar( "ds_debug", Help = "Are we in debug mode?" )]
	public static bool InDebug { get; set; } = false;

	[ConVar( "ds_observing", Help = "Observe specific chunk" ), Range( -1, 64 )]
	public static int ObservedChunkIndex { get; set; } = -1;

	private void UpdateDebug()
	{
		if ( InDebug is false )
			return;

		DebugOverlaySystem debug = SnowTerrain.DebugOverlay;

		if ( ObservedChunkIndex >= 0 && ObservedChunkIndex != Id )
			return;

		debug.Box( Bounds, Color.Red );
		debug.Text( Bounds.Center, $"Chunk: {Id}" );
	}
}
