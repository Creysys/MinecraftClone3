#version 430

layout(location = 0) in vec3 inPosition;

layout(location = 0) out vec2 outTexCoord;

layout(location = 0) uniform vec4 uRect;
layout(location = 1) uniform vec4 uUVRect;

void main()		
{
	vec2 n = inPosition.xy*0.5 + 0.5;
	gl_Position = vec4((uRect.xy + n*(uRect.zw - uRect.xy)) * vec2(1, -1), inPosition.z, 1);

	outTexCoord = uUVRect.xy + n*(uUVRect.zw - uUVRect.xy);
} 