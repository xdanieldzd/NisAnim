#version 150

layout(location = 0) in vec3 vertexPosition;
layout(location = 1) in vec3 vertexNormal;
layout(location = 2) in vec4 vertexColor;
layout(location = 3) in vec2 vertexTextureCoord;

uniform mat4 projectionMatrix, modelviewMatrix;
uniform mat4 objectMatrix;

out vec4 fragmentColor;
out vec2 fragmentTextureCoord;

void main()
{
	fragmentTextureCoord = vertexTextureCoord;
	fragmentColor = vertexColor;

	gl_Position = (projectionMatrix * (modelviewMatrix * objectMatrix)) * vec4(vertexPosition, 1.0);
}
