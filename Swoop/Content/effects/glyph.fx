#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

Texture2D char_map_texture;
sampler2D char_map_sampler = sampler_state
{
    Texture = <char_map_texture>;
};


struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 TextureCoordinates : TEXCOORD0;
};


float4  MainPS(VertexShaderOutput input) : COLOR
{
    float4 tex = tex2D(char_map_sampler, input.TextureCoordinates);
	
    float value = tex.r;
    float a = 1.0-tex.g;
    
    tex.r = value;
    tex.g = value;
    tex.b = value;
    tex.a = a;
	
    return tex * input.Color;
	
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};