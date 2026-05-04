using Sandbox;

namespace Snow.Chunk;

internal sealed partial class SnowTerrainChunk
{
	internal CameraComponent ColliderCamera;

	private void CreateTerrainCamera()
	{
		GameObject cameraContainer = new( SnowTerrain.Terrain.GameObject, true, $"CameraContainer-{Id}" );

		ColliderCamera = cameraContainer.AddComponent<CameraComponent>();
		ColliderCamera.IsMainCamera = false;
		ColliderCamera.Orthographic = true;
		ColliderCamera.ZNear = 0;
		ColliderCamera.OrthographicHeight = TerrainStorage.TerrainSize / SnowTerrain.Division;
		ColliderCamera.LocalRotation = Rotation.From( new Angles( -90f, 90f, 0f ) );
		ColliderCamera.BackgroundColor = Color.Black;
		ColliderCamera.EnablePostProcessing = false;
		ColliderCamera.RenderTags = SnowTerrain.SnowColliderTags;

		ColliderCamera.CustomSize = new Vector2( HighTextureSize, HighTextureSize);
		ColliderCamera.RenderTarget = RenderTarget;

		UpdateTerrainCameraPosition();
	}

	internal void UpdateTerrainCameraPosition()
	{
		if ( ColliderCamera.IsValid() is false )
			return;

		Vector3 boundsCenter = Bounds.Center;
		boundsCenter.z = 0f;

		ColliderCamera.LocalPosition = boundsCenter;
		ColliderCamera.ZFar = TerrainStorage.TerrainHeight + SnowTerrain.SnowHeight;
	}
}
