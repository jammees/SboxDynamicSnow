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

			if ( _colliderCamera.IsValid() is false )
				return;

			_colliderCamera.RenderTags = field;
		}
	}

	[Property]
	[Group( "Config" )]
	public SupportedTextureSize HighMaskSize { get; set; } = SupportedTextureSize.Big;

	private const string TERRAIN_COMPUTE_PATH = "shaders/snowterraincompute.shader";

	private int HighTextureSize => HighMaskSize.AsInt();

	private GpuBuffer<SnowTerrainBuffer> _terrainBuffer;
	private CommandList _renderList;
	private CameraComponent _colliderCamera;
	private Texture _processedMask;
	private Texture _renderTarget;

	protected override void OnEnabled()
	{
		CreateTextures();
		CreateTerrainCamera();

		_terrainBuffer = new( 1, GpuBuffer.UsageFlags.Structured, "SnowTerrainBuffer" );

		_renderList = new();
		_colliderCamera.AddCommandList( _renderList, Stage.AfterDepthPrepass, 1000 );

		Log.Info( $"Added command list" );
	}

	protected override void OnDisabled()
	{
		_colliderCamera?.RemoveCommandList( _renderList );
		_colliderCamera?.DestroyGameObject();
		_terrainBuffer?.Dispose();

		CreateTextures( disposeOnly: true );
	}

	protected override void OnUpdate()
	{
		ComputeShader processorShader = new( TERRAIN_COMPUTE_PATH );

		_renderList?.Reset();

		_renderList.Attributes.GrabDepthTexture( "Depth" );
		_renderList.Attributes.Set( "SnowMask", _processedMask );

		_renderList.DispatchCompute( processorShader, HighTextureSize, HighTextureSize, 1 );

		SnowTerrainBuffer bufferData = new()
		{
			Mask = _processedMask.Index,
			SnowHeight = SnowHeight,
		};
		_terrainBuffer.SetData( new List<SnowTerrainBuffer>() { bufferData } );

		Scene.RenderAttributes.Set( "SnowTerrainBuffer", _terrainBuffer );
	}

	private void CreateTextures( bool disposeOnly = false )
	{
		_processedMask?.Dispose();
		_renderTarget?.Dispose();

		if ( disposeOnly is true )
			return;

		_processedMask = Texture.Create( HighTextureSize, HighTextureSize, ImageFormat.R16 )
			.WithName( "ProcessedSnowMask" )
			.WithAnonymous( false )
			.WithDynamicUsage()
			.WithUAVBinding()
			.Finish();

		_renderTarget = Texture.CreateRenderTarget()
			.WithMSAA( MultisampleAmount.MultisampleNone )
			.WithSize( HighTextureSize, HighTextureSize )
			.Create( name: "SnowColliderRenderTarget", anonymous: false );
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
		_colliderCamera.RenderTags = SnowColliderTags;

		_colliderCamera.CustomSize = new Vector2( HighTextureSize, HighTextureSize );
		_colliderCamera.RenderTarget = _renderTarget;

		UpdateTerrainCameraPosition();
	}

	private void UpdateTerrainCameraPosition()
	{
		if ( _colliderCamera.IsValid() is false )
			return;

		_colliderCamera.LocalPosition = new Vector3( Terrain.Storage.TerrainSize * 0.5f ).WithZ( 0 );
		_colliderCamera.ZFar = Terrain.Storage.TerrainHeight + SnowHeight;
	}
}
