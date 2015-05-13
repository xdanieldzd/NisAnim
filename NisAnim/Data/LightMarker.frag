#version 150

in vec4 geom_fragmentColor;

void main()
{
	gl_FragColor = geom_fragmentColor;
}
