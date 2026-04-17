using Sandbox;
using Sandbox.Rendering;
using Snow.Buffers;
using System.Collections.Generic;

namespace Snow.Terrain;

public sealed partial class SnowTerrain
{
	private int HighTextureSize => HighMaskSize.AsInt();

	private GpuBuffer<SnowTerrainBuffer> _terrainBuffer;
	private CommandList _renderList;

	private bool _isMaskCleared;

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
		_renderList.Attributes.Set( "TerrainControl", Terrain.ControlMap );
		_renderList.Attributes.Set( "SnowHeight", SnowHeight );
		_renderList.Attributes.Set( "TerrainHeight", Terrain.Storage.TerrainHeight );
		_renderList.Attributes.Set( "HeightmapUvScale", GetInverseDimensionSize() );
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

	private float GetInverseDimensionSize()
	{
		float heightmapResolution = Terrain.Storage.Resolution;
		return heightmapResolution / (float)HighTextureSize;
	}
}
