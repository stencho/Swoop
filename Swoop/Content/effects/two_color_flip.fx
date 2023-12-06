#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float4 color_a; float4 color_b;
float2 top_left; float2 bottom_right;

Texture2D screen_texture;
sampler2D screen_texture_sampler = sampler_state {
	Texture = <screen_texture>;	
};

Texture2D SpriteTexture;

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
	float4 color = input.Color * float4(0,0,0,1);
	color.xz = input.TextureCoordinates;
	float4 col_s = tex2D(screen_texture_sampler, input.TextureCoordinates);
	return col_s;
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};