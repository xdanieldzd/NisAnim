#version 150

layout(location = 0) in vec3 vertexPosition;
layout(location = 1) in vec3 vertexNormal;
layout(location = 2) in vec4 vertexColor;
layout(location = 3) in vec2 vertexTextureCoord;

uniform mat4 projectionMatrix, modelviewMatrix;
uniform mat4 objectMatrix, baseMatrix;

uniform vec4 surfaceColor;

out vec4 vert_surfaceColor;
out vec4 vert_basePosition;

void main()
{
	vert_surfaceColor = surfaceColor;
	vert_basePosition = (projectionMatrix * (modelviewMatrix * baseMatrix)) * vec4(vertexPosition, 1.0);
	
	gl_Position = (projectionMatrix * (modelviewMatrix * objectMatrix)) * vec4(vertexPosition, 1.0);
}
