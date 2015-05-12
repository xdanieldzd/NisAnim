#version 150

in vec4 fragmentColor;
in vec2 fragmentTextureCoord;

uniform sampler2D textureMap0;

out vec4 color;

void main()
{
	vec4 textureColor = texture2D(textureMap0, fragmentTextureCoord);

	gl_FragColor = (fragmentColor * textureColor);
}
