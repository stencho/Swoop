#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

Texture2D screen_pos_texture;
sampler2D screen_pos_texture_sampler = sampler_state
{
	Texture = <screen_pos_texture>;
	
    magfilter = POINT;
    minfilter = POINT;
    mipfilter = POINT;
    ADDRESSU = CLAMP;
    ADDRESSV = CLAMP;
};

Texture2D screen_texture;
sampler2D screen_texture_sampler = sampler_state
{
	Texture = <screen_texture>;	
    magfilter = POINT;
    minfilter = POINT;
    mipfilter = POINT;
    ADDRESSU = CLAMP;
    ADDRESSV = CLAMP;
};

float4 tint = float4(1, 1, 1, 1);
float4 bg = float4(1, 1, 1, 1);

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float2 TextureCoordinates : TEXCOORD0;
};

float4 MainPS(VertexShaderOutput input) : COLOR
{
	float2 screen_pos = tex2D(screen_pos_texture_sampler, input.TextureCoordinates);
	float4 screen_tex = tex2D(screen_texture_sampler, screen_pos);

    float4 col = bg;

    if (screen_tex.a > 0)
        col.rgb = screen_tex.rgb * tint.rgb;
	
    return col;
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};