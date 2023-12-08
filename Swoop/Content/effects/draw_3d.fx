#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float4x4 world; 
float4x4 view; 
float4x4 projection;

Texture2D tx;
sampler2D tx_sampler {
	Texture = <tx>;
};

float4 tint = float4(1,1,1,1);

struct VertexShaderInput
{
	float4 Position : POSITION0;
	float4 TextureCoordinates : TEXCOORD0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 TextureCoordinates : TEXCOORD0;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;

	float4x4 wvp = mul(world, mul(view, projection));

	output.Position = mul(input.Position, wvp);
	output.TextureCoordinates = input.TextureCoordinates;

	return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
	return (1,0,0,1);
}

technique BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};