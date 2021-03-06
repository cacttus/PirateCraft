#include "v_glsl_version.glsl"

//20151122
//20161127
layout(location = 0) in vec3 _v301;//_vertex;
layout(location = 1) in vec2 _x201;//_tcoord;

uniform mat4 _ufMatrix_Projection;
out vec2 _tcoordOut;

void main()
{
	_tcoordOut = _x201;
	gl_Position =  _ufMatrix_Projection * vec4(_v301.xy,0,1);
}
