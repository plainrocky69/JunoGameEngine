#include <metal_stdlib>
using namespace metal;

struct PS_INPUT
{
    float4 pos [[ position ]];
    float4 col;
    float2 uv;
};

fragment float4 FS(
    PS_INPUT input [[ stage_in ]],
    texture2d<float> FontTexture [[ texture(2) ]],
    sampler FontSampler [[ sampler(1) ]])
{
    return input.col * FontTexture.sample(FontSampler, input.uv);
}
