using Sandbox;
using Snow.Enums;
using System;

namespace Snow.Terrain;

public sealed partial class SnowTerrain
{
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
	public SupportedTextureSizes HighMaskSize { get; set; } = SupportedTextureSizes.Medium;
}
