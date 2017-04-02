#version 430

layout(location = 0) in vec4 inTexCoord;
layout(location = 1) in vec4 inOverlayTexCoord;
layout(location = 2) in vec4 inNormal;
layout(location = 3) in vec4 inColor;
layout(location = 4) in vec4 inOverlayColor;

layout(location = 0) out vec4 outDiffuse;
layout(location = 1) out vec4 outNormal;

layout(binding = 0) uniform sampler2DArray uTextures16;
layout(binding = 1) uniform sampler2DArray uTextures64;
layout(binding = 2) uniform sampler2DArray uTextures256;
layout(binding = 3) uniform sampler2DArray uTextures1024;

layout(location = 12) uniform bool uCutoff;

vec4 GetDiffuse()
{
	if (inNormal.w == 1) return inColor;

	vec4 texColor = vec4(0);
	if (inTexCoord.x >= 0)
	{
		if(inTexCoord.w == 0) texColor = texture(uTextures16, inTexCoord.xyz);
		else if(inTexCoord.w == 1) texColor = texture(uTextures64, inTexCoord.xyz);
		else if(inTexCoord.w == 2) texColor = texture(uTextures256, inTexCoord.xyz);
		else if(inTexCoord.w == 3) texColor = texture(uTextures1024, inTexCoord.xyz);
		texColor *= inColor;
	}

	vec4 overlayTexColor = vec4(0);
	if (inOverlayTexCoord.x >= 0)
	{
		if (inOverlayTexCoord.w == 0) overlayTexColor = texture(uTextures16, inOverlayTexCoord.xyz);
		else if (inOverlayTexCoord.w == 1) overlayTexColor = texture(uTextures64, inOverlayTexCoord.xyz);
		else if (inOverlayTexCoord.w == 2) overlayTexColor = texture(uTextures256, inOverlayTexCoord.xyz);
		else if (inOverlayTexCoord.w == 3) overlayTexColor = texture(uTextures1024, inOverlayTexCoord.xyz);
		overlayTexColor *= inOverlayColor;
	}

	texColor.rgb = overlayTexColor.rgb * overlayTexColor.a + texColor.rgb * (1 - overlayTexColor.a);
	texColor.a += overlayTexColor.a;
	
	if(uCutoff && texColor.a < 0.5) discard;
	
	return texColor;
}

vec4 EncodeNormal(vec4 normal)
{
	return normal*0.5 + 0.5;
}

void main()		
{
	vec4 diffuse = GetDiffuse();

	outDiffuse = diffuse;
	outNormal = EncodeNormal(inNormal);
 }