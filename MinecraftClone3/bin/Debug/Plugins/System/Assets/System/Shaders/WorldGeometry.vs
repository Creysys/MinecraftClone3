#version 430

layout(location = 0) in vec3 inPosition;
layout(location = 1) in vec4 inTexCoord;
layout(location = 2) in vec4 inNormal;
layout(location = 3) in vec3 inColor;
layout(location = 4) in vec3 inLight;

layout(location = 0) out vec4 outTexCoord;
layout(location = 1) out vec4 outNormal;
layout(location = 2) out vec3 outColor;
layout(location = 3) out vec3 outLight;

layout(location = 0) uniform mat4 uWorld;
layout(location = 4) uniform mat4 uView;
layout(location = 8) uniform mat4 uProjection;

void main()		
{
	gl_Position = uProjection*uView*uWorld*vec4(inPosition, 1);

	outTexCoord = inTexCoord;
	outNormal = inNormal;
	outColor = inColor;
	outLight = inLight;
} 