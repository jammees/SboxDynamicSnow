using Sandbox;

namespace Snow.Terrain;

internal sealed partial class SnowTerrainChunk
{
	private CameraComponent _colliderCamera;

	private void CreateTerrainCamera()
	{
		GameObject cameraContainer = new( SnowTerrain.Terrain.GameObject, true, "CameraContainer" );

		_colliderCamera = cameraContainer.AddComponent<CameraComponent>();
		_colliderCamera.IsMainCamera = false;
		_colliderCamera.Orthographic = true;
		_colliderCamera.ZNear = 0;
		_colliderCamera.OrthographicHeight = TerrainStorage.TerrainSize;
		_colliderCamera.LocalRotation = Rotation.From( new Angles( -90f, 90f, 0f ) );
		_colliderCamera.BackgroundColor = Color.Black;
		_colliderCamera.EnablePostProcessing = false;
		_colliderCamera.RenderTags = SnowTerrain.SnowColliderTags;

		//_colliderCamera.CustomSize = new Vector2( SnowTerrain.HighMaskSize, SnowTerrain.HighTextureSize );
		//_colliderCamera.RenderTarget = _renderTarget;

		UpdateTerrainCameraPosition();
	}

	private void UpdateTerrainCameraPosition()
	{
		if ( _colliderCamera.IsValid() is false )
			return;

		_colliderCamera.LocalPosition = new Vector3( TerrainStorage.TerrainSize * 0.5f ).WithZ( 0 );
		_colliderCamera.ZFar = TerrainStorage.TerrainHeight + SnowTerrain.SnowHeight;
	}
}
