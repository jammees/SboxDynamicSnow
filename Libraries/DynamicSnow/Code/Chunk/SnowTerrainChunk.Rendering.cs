using Sandbox;
using Sandbox.Rendering;
using Snow.Buffers;
using Snow.Utility;
using System.Collections.Generic;

namespace Snow.Terrain;

internal sealed partial class SnowTerrainChunk
{
	private ComputeShader UpdateDeformation;
	private ComputeShader UpdateSnowMask;

	private CommandList _renderList;

	private bool _isMaskCleared;

	private void UpdateInternal()
	{
		_renderList?.Reset();

		// probably should just pass in the texture indexes instead of the actual texture
		_renderList.Attributes.Set( "DeformationMask", RawDeformationMask );
		_renderList.Attributes.Set( "TerrainHeightmap", Terrain.HeightMap );
		_renderList.Attributes.Set( "TerrainControl", Terrain.ControlMap );
		_renderList.Attributes.Set( "SnowHeight", SnowTerrain.SnowHeight );
		_renderList.Attributes.Set( "TerrainHeight", TerrainStorage.TerrainHeight );
		_renderList.Attributes.Set( "HeightmapUvScale", GetInverseDimensionSize() );
		_renderList.Attributes.Set( "SnowMask", SnowMask );

		if ( _isMaskCleared is false )
		{
			ComputeShader clearDeformation = new( Constants.CLEAR_DEFORMATION_COMPUTE );

			_isMaskCleared = true;

			_renderList.DispatchCompute( clearDeformation, HighTextureSize, HighTextureSize, 1 );
			_renderList.UavBarrier( RawDeformationMask );
		}

		_renderList.DispatchCompute( UpdateDeformation, HighTextureSize, HighTextureSize, 1 );
		_renderList.UavBarrier( RawDeformationMask );
		_renderList.DispatchCompute( UpdateSnowMask, HighTextureSize, HighTextureSize, 1 );

		//UploadToGPU();
	}

	private float GetInverseDimensionSize()
	{
		float heightmapResolution = TerrainStorage.Resolution;
		return heightmapResolution / (float)HighTextureSize;
	}

	private void CreateComputes()
	{
		UpdateDeformation = new( Constants.UPDATE_DEFORMATION_COMPUTE );
		UpdateSnowMask = new( Constants.UPDATE_MASK_COMPUTE );
	}
}
