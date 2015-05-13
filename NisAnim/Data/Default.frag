#version 150

in vec3 fragmentPosition;
in vec3 fragmentNormal;
in vec4 fragmentColor;
in vec2 fragmentTextureCoord;

uniform mat4 modelviewMatrix;
uniform sampler2D textureMap0;

uniform bool enableLight;
uniform vec3 lightPosition = vec3(0.0, 0.0, 0.0);

uniform vec4 lightAmbient = vec4(0.5, 0.5, 0.5, 1.0);
uniform vec4 lightDiffuse = vec4(0.25, 0.25, 0.25, 1.0);
uniform vec4 lightSpecular = vec4(1.0, 1.0, 1.0, 1.0);
uniform float lightShininess = 5.0;

void main()
{
	vec4 surfaceColor = (fragmentColor * texture2D(textureMap0, fragmentTextureCoord));

	if(enableLight)
	{
		vec3 normal = normalize(transpose(inverse(mat3(modelviewMatrix))) * fragmentNormal);
		
		vec3 surfacePos = vec3(modelviewMatrix * vec4(fragmentPosition, 1.0));
		vec3 surfaceToLight = normalize(lightPosition - surfacePos);
		vec3 halfVector = normalize(surfaceToLight + normalize(normal));

		float diffuseCoefficient = max(0.0, dot(normal, surfaceToLight));
		float specularCoefficient = pow(max(0.0, dot(normal, halfVector)), lightShininess);
		
		gl_FragColor = (surfaceColor * lightAmbient) + (diffuseCoefficient * lightDiffuse) + ((specularCoefficient * lightSpecular) * 0.25);
	}
	else
	{
		gl_FragColor = surfaceColor;
	}
}
