#version 430

layout(location = 0) out vec4 outDiffuse;
layout(location = 1) out vec4 outNormal;

layout(location = 4) uniform vec4 uColor;

void main()		
{
	outDiffuse = uColor;
	outNormal = vec4(0, 0, 0, 1);
}