#version 430

layout(location = 0) in vec3 inPosition;

layout(location = 0) out vec2 outTexCoord;

void main()		
{
	gl_Position = vec4(inPosition, 1);

	outTexCoord = vec2(inPosition.x*0.5 + 0.5, 0.5 - inPosition.y*0.5);
} 