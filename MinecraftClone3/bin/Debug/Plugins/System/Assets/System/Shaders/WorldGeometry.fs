#version 430

layout(location = 0) in vec4 inTexCoord;
layout(location = 1) in vec4 inNormal;
layout(location = 2) in vec3 inColor;
layout(location = 3) in vec3 inLight;

layout(location = 0) out vec4 outDiffuse;
layout(location = 1) out vec4 outNormal;
layout(location = 2) out vec3 outLight;

layout(binding = 0) uniform sampler2DArray uTextures16;
layout(binding = 1) uniform sampler2DArray uTextures64;
layout(binding = 2) uniform sampler2DArray uTextures256;
layout(binding = 3) uniform sampler2DArray uTextures1024;

layout(location = 12) uniform bool uCutoff;

vec4 GetDiffuse()
{
	//If w value of normal is 1 dont apply shading (eg. bounding boxes)
	if (inNormal.w == 1) return vec4(inColor, 1);

	//Get color from the right texture array
	vec4 texColor = vec4(0);
	if(inTexCoord.w == 0) texColor = texture(uTextures16, inTexCoord.xyz);
	else if(inTexCoord.w == 1) texColor = texture(uTextures64, inTexCoord.xyz);
	else if(inTexCoord.w == 2) texColor = texture(uTextures256, inTexCoord.xyz);
	else if(inTexCoord.w == 3) texColor = texture(uTextures1024, inTexCoord.xyz);
	texColor.rgb *= inColor;
	
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
	outLight = inLight;
 }