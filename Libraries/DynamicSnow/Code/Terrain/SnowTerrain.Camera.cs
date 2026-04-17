using Sandbox;

namespace Snow.Terrain;

public sealed partial class SnowTerrain
{
	private CameraComponent _colliderCamera;

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
