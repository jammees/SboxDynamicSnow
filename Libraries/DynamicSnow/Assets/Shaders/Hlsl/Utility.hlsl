#ifndef TERRAIN_PROCESSING_UTILITY_HLSL
#define TERRAIN_PROCESSING_UTILITY_HLSL

// Really similar to ones that are defined inside of system.fxc
// only difference is that this should accept a texture that already has
// a "type" defined for it
int2 GetFloatTextureDimensions( Texture2D<float> texture, int mipLevel )
{
    int2 dimension;
    int levels;
    texture.GetDimensions( mipLevel, dimension.x, dimension.y, levels );
    return dimension;
}

int2 GetFloatTextureDimensions( RWTexture2D<float> texture )
{
    int2 dimension;
    texture.GetDimensions( dimension.x, dimension.y );
    return dimension;
}

#endif