#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

Texture2D SpriteTexture;

float2 size;
float line_width = 1;
bool fill = false;

sampler2D SpriteTextureSampler = sampler_state
{
	Texture = <SpriteTexture>;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 TextureCoordinates : TEXCOORD0;
};

float4 MainPS(VertexShaderOutput input) : COLOR
{
	float4 c = tex2D(SpriteTextureSampler,input.TextureCoordinates) * input.Color;
	float2 px = 1/size;

	if (length(input.TextureCoordinates - float2(0.5, 0.5)) > 0.5) {
		clip(-1);
	} 

	if (!fill && length(input.TextureCoordinates - float2(0.5, 0.5)) < 0.5 - (px.x * line_width)){
		clip(-1);
	} 

	return c;
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};