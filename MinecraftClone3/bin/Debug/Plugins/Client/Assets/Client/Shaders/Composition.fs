#version 430

layout(location = 0) in vec2 inTexCoord;

layout(location = 0) out vec4 outColor;

layout(binding = 0) uniform sampler2D uDiffuse;
layout(binding = 1) uniform sampler2D uNormal;
layout(binding = 2) uniform sampler2D uDepth;
layout(binding = 3) uniform sampler2D uLight;

vec4 DecodeNormal(vec4 normal)
{
	return normal*2 - 1;
}

void main()		
{
	vec4 diffuse = texture(uDiffuse, inTexCoord);
	if(diffuse.a == 0) discard;

	vec4 light = texture(uLight, inTexCoord);

	outColor = diffuse;
}