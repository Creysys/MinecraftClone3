#version 430

layout(location = 0) in vec4 inTexCoord;

layout(location = 0) out vec4 outColor;

layout(binding = 0) uniform sampler2DArray uTextures16;
layout(binding = 1) uniform sampler2DArray uTextures64;
layout(binding = 2) uniform sampler2DArray uTextures256;
layout(binding = 3) uniform sampler2DArray uTextures1024;

void main()
{
	vec4 texColor = vec4(1, 0, 1, 1);
	if(inTexCoord.w == 0) texColor = texture(uTextures16, inTexCoord.xyz);
	if(inTexCoord.w == 1) texColor = texture(uTextures64, inTexCoord.xyz);
	if(inTexCoord.w == 2) texColor = texture(uTextures256, inTexCoord.xyz);
	if(inTexCoord.w == 3) texColor = texture(uTextures1024, inTexCoord.xyz);

	outColor = texColor;
}