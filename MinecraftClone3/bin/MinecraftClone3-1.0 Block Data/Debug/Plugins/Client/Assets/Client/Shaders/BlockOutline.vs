#version 430

layout(location = 0) in vec3 inPosition;

layout(location = 0) uniform mat4 uTransform;

void main()		
{
	gl_Position = uTransform*vec4(inPosition, 1);
} 