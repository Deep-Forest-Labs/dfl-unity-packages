#ifndef DFL_BAYER_DITHER_INCLUDED
#define DFL_BAYER_DITHER_INCLUDED

static const float Bayer4x4[16] =
{
     0.0 / 16.0,  8.0 / 16.0,  2.0 / 16.0, 10.0 / 16.0,
    12.0 / 16.0,  4.0 / 16.0, 14.0 / 16.0,  6.0 / 16.0,
     3.0 / 16.0, 11.0 / 16.0,  1.0 / 16.0,  9.0 / 16.0,
    15.0 / 16.0,  7.0 / 16.0, 13.0 / 16.0,  5.0 / 16.0
};

float BayerThreshold(uint2 pixel)
{
    uint x = pixel.x & 3u;
    uint y = pixel.y & 3u;
    return Bayer4x4[y * 4u + x];
}

#endif
