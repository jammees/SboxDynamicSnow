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

float CustomGaussianBlur( Texture2D<float> mask, SamplerState sampler, uint2 uv, float2 size )
{
    float fl2PI = 6.28318530718f;
    float flDirections = 16.0f;
    float flQuality = 4.0f;
    float flTaps = 1.0f;

    // Had to use this because the original function used Sample which can't
    // be used outside of fragment also SampleLevel just screwed everything up
    float vColor = mask[uv.xy];

    [unroll]
    for( float d=0.0; d<fl2PI; d+=fl2PI/flDirections)
    {
        [unroll]
        for(float j=1.0/flQuality; j<=1.0; j+=1.0/flQuality)
        {
            flTaps += 1;
            vColor += mask[uv.xy + float2( cos(d), sin(d) ) * size * j];    
        }
    }

    return vColor / flTaps;
}


#endif