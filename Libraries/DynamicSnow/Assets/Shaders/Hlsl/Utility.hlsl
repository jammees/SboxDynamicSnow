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

float CustomGaussianBlur( Texture2D<float> mask, SamplerState sampler, uint2 id, float2 size )
{
    float fl2PI = 6.28318530718f;
    float flDirections = 16.0f;
    float flQuality = 4.0f;
    float flTaps = 1.0f;

    float2 maskSize = GetFloatTextureDimensions( mask, 0 );
    float2 initialUv = id / maskSize;

    float vColor = mask.SampleLevel( sampler, initialUv, 0 );

    [unroll]
    for( float d=0.0; d<fl2PI; d+=fl2PI/flDirections)
    {
        [unroll]
        for(float j=1.0/flQuality; j<=1.0; j+=1.0/flQuality)
        {
            flTaps += 1;
            float2 sampleOffset = float2( cos(d), sin(d) ) * j;
            float2 offsetUv = (id.xy + sampleOffset * size) / maskSize;
            vColor += mask.SampleLevel( sampler, offsetUv, 0 );    
        }
    }

    return vColor / flTaps;
}


#endif