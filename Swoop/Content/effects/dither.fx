#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

Texture2D SpriteTexture;

sampler2D SpriteTextureSampler = sampler_state
{
	Texture = <SpriteTexture>;
};

float4 color_a = float4(1,1,1,1);
float4 color_b = float4(0.5,0.5,0.5,1);

float2 top_left; float2 bottom_right;

float4x4 world; 

bool clip_b = false;

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 texCoord : TEXCOORD0;
};

float4 MainPS(VertexShaderOutput input) : COLOR
{
	float2 size = bottom_right - top_left;
	float2 px = size * input.texCoord.xy;

	int2 px_i = int2(px);

	float4 color = input.Color;

	bool x = px_i.x % 2 == 0;
	bool y = px_i.y % 2 == 0;

	if ((x && y) || (!x && !y)) {
		color *= color_a;
	} else { 		
		if (clip_b) clip(-1);
		color *= color_b;
	}

	return color;
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};