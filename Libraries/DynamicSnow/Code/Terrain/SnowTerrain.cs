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

	protected override void DrawGizmos()
	{
		float terrainSize = Terrain.Storage.TerrainSize;
		float terrainHeight = Terrain.Storage.TerrainHeight;

		float chunkArea = terrainSize / Division;

		using ( Gizmo.Scope( "SnowTerrain" ) )
		{
			Gizmo.Draw.Color = Color.Green;
			Gizmo.Draw.LineThickness = 2f;

			for ( int x = 0; x < Division; x++ )
			{
				for ( int y = 0; y < Division; y++ )
				{
					Vector3 min = Vector3.Zero;
					min = new Vector3( chunkArea * x, chunkArea * y );

					Vector3 max = Vector3.Zero;
					max = new Vector3( chunkArea * (x + 1), chunkArea * (y + 1), terrainHeight );

					BBox bounds = new BBox( min, max );

					Gizmo.Draw.LineBBox( bounds );

					Gizmo.Draw.Text( $"{x+(y*Division)+1}", new Transform( bounds.Center ), size: 20f );
				}
			}
		}
	}

	protected override void OnAwake()
	{
		UpdateDeformation = new( UPDATE_DEFORMATION_COMPUTE );
		UpdateSnowMask = new( UPDATE_MASK_COMPUTE );
	}

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

	protected override void OnUpdate()
	{
		Scene.RenderAttributes.SetCombo( "D_DYNAMIC_SNOW_IN_EDITOR", Game.IsPlaying is false );

		if ( Game.IsPlaying is false )
			return;

		UpdateTerrain();
	}
}
