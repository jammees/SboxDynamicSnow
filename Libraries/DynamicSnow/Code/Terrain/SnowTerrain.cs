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

	private const string CLEAR_DEFORMATION_COMPUTE = "shaders/computes/cleardeformation.shader";
	private const string UPDATE_DEFORMATION_COMPUTE = "shaders/computes/updatedeformation.shader";
	private const string UPDATE_MASK_COMPUTE = "shaders/computes/updatesnowmask.shader";

	private int HighTextureSize => HighMaskSize.AsInt();

	private GpuBuffer<SnowTerrainBuffer> _terrainBuffer;
	private CommandList _renderList;
	private CameraComponent _colliderCamera;
	private Texture _rawDeformationMask;
	private Texture _snowMask;
	private Texture _renderTarget;

	private bool _isMaskCleared;

	protected override void OnEnabled()
	{
		CreateTextures();
		CreateTerrainCamera();

		_isMaskCleared = false;

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
		if ( _rawDeformationMask.IsValid() is false || _snowMask.IsValid() is false )
			return;

		ComputeShader updateDeformation = new( UPDATE_DEFORMATION_COMPUTE );
		ComputeShader updateSnowMask = new( UPDATE_MASK_COMPUTE );

		_renderList?.Reset();

		// probably should just pass in the texture indexes instead of the actual texture
		_renderList.Attributes.Set( "DeformationMask", _rawDeformationMask );
		_renderList.Attributes.Set( "TerrainHeightmap", Terrain.HeightMap );
		_renderList.Attributes.Set( "SnowHeight", SnowHeight );
		_renderList.Attributes.Set( "TerrainHeight", Terrain.Storage.TerrainHeight );
		_renderList.Attributes.Set( "UvScalar", GetInverseDimensionSize() );
		_renderList.Attributes.Set( "SnowMask", _snowMask );

		if ( _isMaskCleared is false )
		{
			ComputeShader clearDeformation = new( CLEAR_DEFORMATION_COMPUTE );

			_isMaskCleared = true;

			_renderList.DispatchCompute( clearDeformation, HighTextureSize, HighTextureSize, 1 );
			_renderList.UavBarrier( _rawDeformationMask );
		}

		_renderList.DispatchCompute( updateDeformation, HighTextureSize, HighTextureSize, 1 );
		_renderList.UavBarrier( _rawDeformationMask );
		_renderList.DispatchCompute( updateSnowMask, HighTextureSize, HighTextureSize, 1 );

		SnowTerrainBuffer bufferData = new()
		{
			Mask = _snowMask.Index,
			SnowHeight = SnowHeight,
		};
		_terrainBuffer.SetData( new List<SnowTerrainBuffer>() { bufferData } );
		Scene.RenderAttributes.Set( "SnowTerrainBuffer", _terrainBuffer );
	}

	private void CreateTextures( bool disposeOnly = false )
	{
		_rawDeformationMask?.Dispose();
		_snowMask?.Dispose();
		_renderTarget?.Dispose();

		if ( disposeOnly is true )
			return;

		_rawDeformationMask = Texture.Create( HighTextureSize, HighTextureSize, ImageFormat.R16 )
			.WithName( "RawDeformationSnowMask" )
			.WithAnonymous( false )
			.WithGPUOnlyUsage()
			.WithUAVBinding()
			.Finish();

		_snowMask = Texture.Create( HighTextureSize, HighTextureSize, ImageFormat.A8 )
			.WithName( "ProcessedSnowMask" )
			.WithAnonymous( false )
			.WithGPUOnlyUsage()
			.WithUAVBinding()
			.Finish();

		// NOTE: Probably should not use RGBA8888 format here
		// from testing A8 seemed to be fine enough
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

	private float GetInverseDimensionSize()
	{
		float heightmapResolution = Terrain.Storage.Resolution;
		return heightmapResolution / HighTextureSize;
	}
}
