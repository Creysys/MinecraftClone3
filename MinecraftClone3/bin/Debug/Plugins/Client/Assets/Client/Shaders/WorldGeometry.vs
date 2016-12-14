#version 430

layout(location = 0) in vec3 inPosition;
layout(location = 1) in vec4 inTexCoord;
layout(location = 2) in vec3 inNormal;

layout(location = 0) out vec4 outTexCoord;
layout(location = 1) out vec3 outNormal;

layout(location = 0) uniform mat4 uWorld;
layout(location = 4) uniform mat4 uView;
layout(location = 8) uniform mat4 uProjection;

void main()		
{
	gl_Position = uProjection*uView*uWorld*vec4(inPosition, 1);

	outTexCoord = inTexCoord;
	outNormal = inNormal;
} 