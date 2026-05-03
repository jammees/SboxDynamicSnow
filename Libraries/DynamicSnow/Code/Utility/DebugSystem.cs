using Sandbox;

namespace Snow.Utility;

internal sealed class DebugSystem : GameObjectSystem<DebugSystem>
{
	private DebugOverlaySystem Debug => Scene.DebugOverlay;
	
	private float _offset = 0f;

	public DebugSystem( Scene scene ) : base( scene )
	{
		Listen( Stage.StartUpdate, 0, () => _offset = 0, "Reset offset" );
	}

	public void Add( string text )
	{
		Debug.ScreenText( new Vector2( 50f, 150f + _offset ), text, flags: TextFlag.LeftCenter );
		_offset += 30f;
	}
}
