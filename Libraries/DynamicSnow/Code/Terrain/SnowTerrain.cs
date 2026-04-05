using Sandbox;
using Sandbox.Rendering;
using System;
using System.Collections.Generic;

namespace Snow.Terrain;

public sealed class SnowTerrain : Component
{
	/// <summary>
	/// Reference to the terrain component.
	/// </summary>
	[Property]
	[RequireComponent]
	public Sandbox.Terrain Terrain { get; set; }

	[Property]
	public CameraComponent ObserverCamera { get; set; }

	/// <summary>
	/// How high should the snow be. Recommended to keep this
	/// relatively low to prevent the player camera clipping into
	/// the rendered mesh when in first-person.
	/// </summary>
	[Property]
	[Group( "Config" )]
	[Range( 0f, 100f ), Step( 0.1f )]
	public float SnowHeight
	{
		get => field;
		set
		{
			field = MathF.Max( value, 0f );
			UpdateTerrainCameraPosition();
		}
	}

	[Property]
	[Group( "Config" )]
	[Range( 0f, 2000f ), Step( 1f )]
	public float HighRenderDistance = 20f;

	[Property]
	[Group( "Config" )]
	[Range( 0f, 2000f ), Step( 1f )]
	public float BlendStartDistance = 20f;

	/// <summary>
	/// Objects with these tags can collide with the snow and
	/// deform it.
	/// </summary>
	[Property]
	[Group( "Config" )]
	public TagSet SnowColliderTags
	{
		get => field;
		set
		{
			field = value;
			UpdateTerrainCameraPosition();
		}
	}

	[Property]
	[Group( "Config" )]
	public SupportedTextureSize LowMaskSize { get; set; } = SupportedTextureSize.Small;

	[Property]
	[Group( "Config" )]
	public SupportedTextureSize HighMaskSize { get; set; } = SupportedTextureSize.Big;

	private const string TERRAIN_COMPUTE_PATH = "shaders/snowterraincompute.shader";
	private const string DEBUG_LOW_MASK_PATH = "showcase/textures/debug/lowmask512.vtex";
	private const string DEBUG_HIGH_MASK_PATH = "showcase/textures/debug/highmask2048.vtex";

	private int LowTextureSize => LowMaskSize.AsInt();
	private int HighTextureSize => HighMaskSize.AsInt();

	private GpuBuffer<SnowTerrainBuffer> _terrainBuffer;
	private CommandList _renderList;
	private CameraComponent _colliderCamera;
	private Texture _maskLow;
	private Texture _maskHigh;

	protected override void OnEnabled()
	{
		CreateTerrainCamera();
		CreateTextures();

		_terrainBuffer = new( 1, GpuBuffer.UsageFlags.Structured, "SnowTerrainBuffer" );

		_renderList = new();
		_colliderCamera.AddCommandList( _renderList, Stage.AfterDepthPrepass, 100 );

		Log.Info( $"Added command list" );
	}

	protected override void OnDisabled()
	{
		_colliderCamera?.RemoveCommandList( _renderList );
		_colliderCamera?.DestroyGameObject();
		_terrainBuffer?.Dispose();

		_maskLow?.Dispose();
		_maskHigh?.Dispose();
	}

	protected override void OnUpdate()
	{
		ComputeShader processorShader = new( TERRAIN_COMPUTE_PATH );

		_renderList?.Reset();

		SnowTerrainBuffer bufferData = new()
		{
			PlayerCamera = ObserverCamera.WorldPosition,
			MaskLow = _maskLow.Index,
			MaskHigh = _maskHigh.Index,
			SnowHeight = SnowHeight,
			BlendStartDistance = BlendStartDistance,
			HighRenderDistance = HighRenderDistance,
		};
		_terrainBuffer.SetData( new List<SnowTerrainBuffer>() { bufferData } );

		Scene.RenderAttributes.Set( "SnowTerrainBuffer", _terrainBuffer );
	}

	private void CreateTextures()
	{
		_maskLow?.Dispose();
		_maskHigh?.Dispose();

		_maskLow = Texture.LoadFromFileSystem( DEBUG_LOW_MASK_PATH, FileSystem.Mounted );
		_maskHigh = Texture.LoadFromFileSystem( DEBUG_HIGH_MASK_PATH, FileSystem.Mounted );
	}

	private void CreateTerrainCamera()
	{
		GameObject cameraContainer = new( Terrain.GameObject, true, "CameraContainer" );

		_colliderCamera = cameraContainer.AddComponent<CameraComponent>();
		_colliderCamera.IsMainCamera = false;
		_colliderCamera.Orthographic = true;
		_colliderCamera.ZNear = 0;
		_colliderCamera.OrthographicHeight = Terrain.Storage.TerrainSize;
		_colliderCamera.LocalRotation = Rotation.From( new Angles( -90f, 90f, 0f ) );
		_colliderCamera.BackgroundColor = Color.Black;
		_colliderCamera.EnablePostProcessing = false;

		UpdateTerrainCameraPosition();

		Log.Info( "Created and updated camera" );
	}

	private void UpdateTerrainCameraPosition()
	{
		if ( _colliderCamera.IsValid() is false )
			return;

		_colliderCamera.LocalPosition = new Vector3( Terrain.Storage.TerrainSize * 0.5f ).WithZ( 0 );
		_colliderCamera.ZFar = Terrain.Storage.TerrainHeight + SnowHeight;
	}
}
