sampler inputTexture;

float4 MainPS(float2 input: TEXCOORD0) : COLOR0
{
	float4 color = tex2D(inputTexture, input);
	if (color.r == 0x08 && color.g == 0 && color.b == 0)
		color.a = 0x00
	return color
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile ps_3_0 MainPS();
	}
};
