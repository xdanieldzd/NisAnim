#version 150

in vec3 fragmentPosition;
in vec3 fragmentNormal;
in vec4 fragmentColor;
in vec2 fragmentTextureCoord;

uniform mat4 modelviewMatrix;
uniform sampler2D textureMap0;

uniform bool enableLight;
uniform vec3 lightPosition, lightIntensities;

out vec4 color;

void main()
{
	vec4 surfaceColor = (fragmentColor * texture2D(textureMap0, fragmentTextureCoord));

	if(enableLight)
	{
		vec3 normal = normalize(transpose(inverse(mat3(modelviewMatrix))) * fragmentNormal);
		
		vec3 surfacePos = vec3(modelviewMatrix * vec4(fragmentPosition, 1.0));
		vec3 surfaceToLight = normalize(lightPosition - surfacePos);

		float diffuseCoefficient = max(0.0, dot(normal, surfaceToLight));
		vec3 diffuse = diffuseCoefficient * surfaceColor.rgb * lightIntensities;

		gl_FragColor = vec4(diffuse.rgb, surfaceColor.a);
	}
	else
	{
		gl_FragColor = surfaceColor;
	}
}
