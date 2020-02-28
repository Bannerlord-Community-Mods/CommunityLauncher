#version 330 core

in vec2 UV;
in vec2 Coord;
out vec4 color;

uniform sampler2D Texture;
uniform sampler2D OverlayTexture;

uniform bool OverlayEnabled;

uniform vec4 InputColor;

uniform bool CircularMaskingEnabled;

uniform float ColorFactor;
uniform float AlphaFactor;
uniform float HueFactor;
uniform float SaturationFactor;
uniform float ValueFactor;

uniform vec2 MaskingCenter;
uniform float MaskingRadius;
uniform float MaskingSmoothingRadius;

uniform vec2 StartCoord;
uniform vec2 Size;
uniform vec2 OverlayOffset;

float blendOverlay2(float base, float blend)
{
	return base < 0.5f ? (2.0f * base * blend) : (1.0f - 2.0f * (1.0f - base)*(1.0f - blend));
}

vec3 blendOverlay2(vec3 base, vec3 blend)
{
	return vec3(blendOverlay2(base.x, blend.x), blendOverlay2(base.y, blend.y), blendOverlay2(base.z, blend.z));
}

vec3 blendOverlay2(vec3 base, vec3 blend, float opacity)
{
	return (blendOverlay2(base, blend) * opacity + base * (1.0f - opacity));
}

float Epsilon = 1e-10;

vec3 HUEtoRGB(float H)
{
	float R = abs(H * 6 - 3) - 1;
	float G = 2 - abs(H * 6 - 2);
	float B = 2 - abs(H * 6 - 4);
	return clamp(vec3(R, G, B), 0.0, 1.0);
}

vec3 RGBtoHCV(vec3 RGB)
{
	vec4 P = (RGB.g < RGB.b ? vec4(RGB.bg, -1.0, 2.0/3.0) : vec4(RGB.gb, 0.0, -1.0/3.0));
	vec4 Q = (RGB.r < P.x) ? vec4(P.xyw, RGB.r) : vec4(RGB.r, P.yzx);
	float C = Q.x - min(Q.w, Q.y);
	float H = abs((Q.w - Q.y) / (6 * C + Epsilon) + Q.z);
	return vec3(H, C, Q.x);
}

vec3 HSVtoRGB(vec3 HSV)
{
	vec3 RGB = HUEtoRGB(HSV.x);
	return ((RGB - 1) * HSV.y + 1) * HSV.z;
}

vec3 RGBtoHSV(vec3 RGB)
{
	vec3 HCV = RGBtoHCV(RGB);
	float S = HCV.y / (HCV.z + Epsilon);
	return vec3(HCV.x, S, HCV.z);
}

void main()
{
	vec4 outColor = texture(Texture, UV);
	
	
	
	if (CircularMaskingEnabled)
	{		
		float x = MaskingCenter.x;
		float y = MaskingCenter.y;
		
		float radius = MaskingRadius;
		float radiusForAlpha = MaskingRadius + MaskingSmoothingRadius;
		float distanceForAlpha = radiusForAlpha - radius;
		
		vec2 relativeCoord = vec2(Coord.x - x, Coord.y - y);
		float coordRadius = sqrt(relativeCoord.x * relativeCoord.x + relativeCoord.y * relativeCoord.y);
		
		if (coordRadius > radiusForAlpha)
		{
			outColor = vec4(0, 0, 0, 0);
		}
		else if( coordRadius > radius)
		{
			float ratio = 1.0f - ((coordRadius - radius) / distanceForAlpha);
			outColor = outColor * ratio;
		}
	}
	
	if(OverlayEnabled)
	{
		vec2 detail_tile = (OverlayOffset + Coord - StartCoord) * (1.0f / 512.0f);
		vec3 detail1 =  texture(OverlayTexture, detail_tile).xyz;
		outColor.xyz = blendOverlay2(outColor.rgb, detail1, 1.0f);
	}
	
	vec4 resultColor = outColor * InputColor;
	
	resultColor.xyz *= ColorFactor;	
	resultColor.w *= AlphaFactor;
	
	vec3 hsv = RGBtoHSV(resultColor.rgb);
	
	hsv.x = mod((hsv.x + HueFactor) + 1.0, 1.0);
	hsv.y = SaturationFactor < 0 ? hsv.y * (1 + SaturationFactor) : hsv.y + (1 - hsv.y) * SaturationFactor;
	hsv.z = ValueFactor < 0 ? hsv.z * (1 + ValueFactor) : hsv.z + (1 - hsv.z) * ValueFactor;
	
	resultColor.rgb = HSVtoRGB(hsv);

	color = resultColor;
}