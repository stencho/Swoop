#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float alpha_scissor = 0.5;
float opacity = 1;
float outline_width = 0;

float blend = 0.05f;

bool invert_map = false;
bool enable_outline = false;

float4 inside_color;
float4 outline_color;
float4 outside_color;

sampler2D SDFs : register(s0) {	
	texture = <SDFTEX>;
	ADDRESSU = WRAP;
	ADDRESSV = WRAP;
};
 
float4 PS(float4 position : SV_Position, float4 color : COLOR0, float2 TexCoords : TEXCOORD0) : COLOR0 {
	float pixel = (tex2D(SDFs, TexCoords).r);	
	float4 rgba = 0;

	//Map uninverted and inside	
	if (pixel > 1-alpha_scissor && !invert_map) {
		rgba = inside_color;
		
		if (enable_outline && pixel < (1-alpha_scissor) + outline_width)
			rgba = outline_color;

	//Map inverted and inside
	} else if (pixel > alpha_scissor && invert_map) {
		rgba = inside_color;

		if (enable_outline && pixel < alpha_scissor + outline_width)
			rgba = outline_color;
	
	//Outside
	} else rgba = outside_color;

	if (!invert_map) {
		if (enable_outline && pixel < (1 - alpha_scissor) + outline_width + blend && pixel >(1 - alpha_scissor) + outline_width - blend) {
			rgba = (inside_color + outline_color) * 0.5f;
		}
		if (pixel < (1 - alpha_scissor) + blend && pixel >(1 - alpha_scissor) - blend) {
			rgba = (outside_color + outline_color) * 0.5f;
		}		
	} 
	
	if (invert_map) {
		if (enable_outline && pixel < alpha_scissor + outline_width + blend && pixel > alpha_scissor + outline_width - blend) {
			rgba = (inside_color + outline_color) * 0.5f;
		}
		if (pixel < alpha_scissor + blend && pixel > alpha_scissor - blend) {
			rgba = (outside_color + outline_color) * 0.5f;
		}
	}
	
	rgba.a *= opacity;
	return rgba;
}

technique Default {
	pass p0 {
		PixelShader = compile PS_SHADERMODEL PS();
	}
}

