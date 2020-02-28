#version 330 core

layout(location = 0) out vec4 color;

uniform vec4 Color = vec4(1, 1, 1, 1);

void main()
{
	color = Color;
}