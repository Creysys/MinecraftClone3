#version 430

layout(location = 0) in vec2 inTexCoord;

layout(location = 0) out vec4 outColor;

layout(location = 0) uniform mat4 uViewProjectionInv;
layout(location = 4) uniform vec3 uLightPosition;
layout(location = 5) uniform vec3 uLightColor;
layout(location = 6) uniform float uLightRange;

layout(binding = 1) uniform sampler2D uNormal;
layout(binding = 2) uniform sampler2D uDepth;

vec4 DecodeNormal(vec4 normal)
{
	return normal*2 - 1;
}

vec3 PositionFromDepth(float depth)
{
	vec4 clipSpace = vec4(inTexCoord*2 - 1, depth, 1);
	vec4 homogenousCoord = uViewProjectionInv*clipSpace;
	return homogenousCoord.xyz/homogenousCoord.w;
}

void main()		
{
	vec4 normal = DecodeNormal(texture(uNormal, inTexCoord));
	float depth = texture(uDepth, inTexCoord).x * 2 - 1;
	vec3 position = PositionFromDepth(depth);

	vec3 pixelToLight = uLightPosition - position;
	float distanceSq = dot(pixelToLight, pixelToLight);
	float normalFactor = max(dot(normalize(pixelToLight), normal.xyz), 0);
	float attenuation = clamp(1 - distanceSq/(uLightRange*uLightRange), 0, 1);

	float ambient = 0.03;
	vec3 color = (normalFactor + ambient)*attenuation*uLightColor;
	color = pow(color, vec3(1/2.2));

	outColor = vec4(color, 1);
}