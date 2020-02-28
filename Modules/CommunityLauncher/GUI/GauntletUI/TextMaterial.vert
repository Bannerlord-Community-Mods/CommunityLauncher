#version 330 core

layout(location = 0) in vec2 vertexPosition;
layout(location = 1) in vec2 vertexUV;

uniform mat4 MVP;

out vec2 UV;
out vec2 Coord;

void main()
{
	gl_Position = MVP * vec4(vertexPosition, 0, 1);	
	
	Coord = vec2(vertexPosition.x, vertexPosition.y);
	UV = vertexUV;
}