using Sandbox;
using Sandbox.Rendering;

namespace Snow.Terrain;

public sealed partial class SnowTerrain : Component, Component.ExecuteInEditor
{
	/// <summary>
	/// Reference to the terrain component.
	/// </summary>
	[Property]
	[RequireComponent]
	public Sandbox.Terrain Terrain { get; set; }

	protected override void OnEnabled()
	{
		if ( Game.IsPlaying is false )
			return;

		CreateTextures();
		CreateTerrainCamera();

		_isMaskCleared = false;

		_terrainBuffer = new( 1, GpuBuffer.UsageFlags.Structured, "SnowTerrainBuffer" );

		_renderList = new();
		_colliderCamera.AddCommandList( _renderList, Stage.AfterDepthPrepass, 1000 );
	}

	protected override void OnDisabled()
	{
		if ( Game.IsPlaying is false )
			return;

		_colliderCamera?.RemoveCommandList( _renderList );
		_colliderCamera?.DestroyGameObject();
		_terrainBuffer?.Dispose();

		CreateTextures( disposeOnly: true );
	}
}
