/* Lighting etc: http://www.tomdalling.com/blog/modern-opengl/08-even-more-lighting-directional-lights-spotlights-multiple-lights/ */

#version 150

#define MAX_LIGHTS 10

in vec3 fragmentPosition;
in vec3 fragmentNormal;
in vec4 fragmentColor;
in vec2 fragmentTextureCoord;

uniform mat4 modelviewMatrix;
uniform vec3 cameraPosition;

uniform sampler2D materialTexture;
uniform float materialShininess;
uniform vec3 materialSpecularColor;

uniform bool enableLight;
uniform int numLights;

uniform struct Light
{
	bool enabled;
	vec4 position;
	vec3 intensities;
	float attenuation;
	float ambientCoefficient;
	float coneAngle;
	vec3 coneDirection;
} lights[MAX_LIGHTS];

vec3 ApplyLight(Light light, vec3 surfaceColor, vec3 normal, vec3 surfacePos, vec3 surfaceToCamera)
{
	vec3 surfaceToLight;
	float attenuation = 1.0;

	if(light.position.w == 0.0)
	{
		//directional light
		surfaceToLight = normalize(light.position.xyz);
		attenuation = 1.0; //no attenuation for directional lights
	}
	else
	{
		//point light
		surfaceToLight = normalize(light.position.xyz - surfacePos);
		float distanceToLight = length(light.position.xyz - surfacePos);
		attenuation = 1.0 / (1.0 + light.attenuation * pow(distanceToLight, 2));

		//cone restrictions (affects attenuation)
		float lightToSurfaceAngle = degrees(acos(dot(-surfaceToLight, normalize(light.coneDirection))));
		if(lightToSurfaceAngle > light.coneAngle)
		{
			attenuation = 0.0;
		}
	}

	//ambient
	vec3 ambient = light.ambientCoefficient * surfaceColor.rgb * light.intensities;

	//diffuse
	float diffuseCoefficient = max(0.0, dot(normal, surfaceToLight));
	vec3 diffuse = diffuseCoefficient * surfaceColor.rgb * light.intensities;
    
	//specular
	float specularCoefficient = 0.0;
	if(diffuseCoefficient > 0.0)
		specularCoefficient = pow(max(0.0, dot(surfaceToCamera, reflect(-surfaceToLight, normal))), materialShininess);
	vec3 specular = specularCoefficient * materialSpecularColor * light.intensities;

	//linear color (color before gamma correction)
	return ambient + attenuation * (diffuse + specular);
}

void main()
{
	vec4 surfaceColor = (fragmentColor * texture2D(materialTexture, fragmentTextureCoord));

	if(enableLight)
	{
		vec3 normal = normalize(transpose(inverse(mat3(modelviewMatrix))) * fragmentNormal);
		vec3 surfacePos = vec3(modelviewMatrix * vec4(fragmentPosition, 1.0));
		vec3 surfaceToCamera = normalize(cameraPosition - surfacePos);

		//combine color from all the lights
		vec3 linearColor = vec3(0);
		for(int i = 0; i < numLights; ++i)
		{
			if(lights[i].enabled)
				linearColor += ApplyLight(lights[i], surfaceColor.rgb, normal, surfacePos, surfaceToCamera);
		}

		//final color (after gamma correction)
		vec3 gamma = vec3(1.0 / 2.2);
		gl_FragColor = vec4(pow(linearColor, gamma), surfaceColor.a);
	}
	else
	{
		gl_FragColor = surfaceColor;
	}
}
