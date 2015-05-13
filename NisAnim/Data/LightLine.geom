#version 150

layout(points) in;
layout(triangle_strip, max_vertices = 20) out;

in vec4 vert_surfaceColor[];
in vec4 vert_basePosition[];

out vec4 geom_fragmentColor;

void main()
{
	float scale = 2.5;
	
	gl_Position = gl_PositionIn[0] - vec4(scale, 0.0, 0.0, 0.0);
	geom_fragmentColor = vert_surfaceColor[0];
	EmitVertex();
	
	gl_Position = gl_PositionIn[0] + vec4(scale, 0.0, 0.0, 0.0);
	geom_fragmentColor = vert_surfaceColor[0];
	EmitVertex();

	gl_Position = vert_basePosition[0];
	geom_fragmentColor = vert_surfaceColor[0];
	EmitVertex();
	
	EndPrimitive();
}
