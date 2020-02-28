#version 330 core

#define saturate(x) clamp(x, 0.0f, 1.0f)
#define PI 3.14159265359

in vec2 UV;
in vec2 Coord;
out vec4 outColor;

uniform sampler2D Texture;
uniform vec4 InputColor;

uniform vec4 OutlineColor;
uniform vec4 GlowColor;

uniform float OutlineAmount;
uniform float ScaleFactor;
uniform float SmoothingConstant;

uniform float GlowRadius;
uniform float Blur;
uniform float ShadowOffset;
uniform float ShadowAngle;

uniform float ColorFactor;
uniform float AlphaFactor;


void main()
{
	vec4 color = InputColor;
	
	float distance = texture(Texture, UV).r;
	
	float smoothing = ScaleFactor;
	
	float SOFT_EDGE_MIN = SmoothingConstant - smoothing;
	float SOFT_EDGE_MAX = SmoothingConstant + smoothing;
	
	float innerfont = smoothstep(SOFT_EDGE_MIN, SOFT_EDGE_MAX, distance);
	float outerfont = 0;
	float shadowfont = 0;
	
	bool OUTLINE = OutlineAmount > 0.0f;
	bool OUTER_GLOW = GlowRadius > 0.0f || Blur > 0.0f;

	if (OUTLINE)
	{
		color = OutlineColor;
	}

	if (OUTER_GLOW)
	{
		color = GlowColor;

		float radian = ShadowAngle * PI / 180.0;
		float sina = sin(radian);
		float cosa = cos(radian);

		float offset = 4.0f * saturate(ShadowOffset) / 4096.0f;

		vec2 GLOW_UV_OFFSET = vec2(offset * cosa, offset * sina);
		float OUTER_GLOW_MIN_DVALUE = SOFT_EDGE_MIN - (SOFT_EDGE_MIN - 0.3f) * 2.8f * saturate(saturate(GlowRadius) / 2 + saturate(Blur));
		float OUTER_GLOW_MAX_DVALUE = SOFT_EDGE_MAX - (SOFT_EDGE_MAX - 0.5f) * 2.8f * saturate(saturate(GlowRadius) / 2);

		float dropDistance = texture(Texture, UV - GLOW_UV_OFFSET).r;
		shadowfont = smoothstep(OUTER_GLOW_MIN_DVALUE, OUTER_GLOW_MAX_DVALUE, dropDistance) * GlowColor.a;
		color.rgb = mix(color.rgb, GlowColor.rgb, shadowfont);
	}

	if (OUTLINE)
	{
		float w = saturate(OutlineAmount) * 0.4f;
		outerfont = smoothstep(SOFT_EDGE_MIN - w, SOFT_EDGE_MAX - w, distance);
		color.rgb = mix(color.rgb, OutlineColor.rgb, outerfont);
	}
	
	color.rgb = mix(color.rgb, InputColor.rgb, innerfont);
	color.a = max(outerfont, max(shadowfont, innerfont));

	color.rgb *= ColorFactor;
	color.a *= AlphaFactor;

	outColor = color;
}