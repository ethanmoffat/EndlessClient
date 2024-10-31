#if OPENGL
    #define SV_POSITION POSITION
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_4_0_level_9_3
    #define PS_SHADERMODEL ps_4_0_level_9_3
#endif

sampler2D TextureSampler : register(s0)
{
    Texture = (Texture);
};

float4 MainPS(float4 position : SV_Position, float4 color : COLOR0, float2 TextureCoordinates : TEXCOORD0) : COLOR0
{
    float4 col = tex2D(TextureSampler, TextureCoordinates);
    col.a = (col.r + col.g + col.b) > 0 ? 1.0 : 0.0;
    return col;
}

technique ClipBlack
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};
