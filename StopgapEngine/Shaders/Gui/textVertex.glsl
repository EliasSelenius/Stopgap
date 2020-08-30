#version 440 core

layout (std140) uniform Camera {
	mat4 projection;
	mat4 view;
};

// vec2 position, vec2 size
//uniform vec2 textPosition;
uniform mat4 model;

layout (location = 0) in vec4 posuv;

out vec2 uv;

void main() {
	
	vec2 vp = posuv.xy;

	//vec2 pos = textPosition + vp;

	gl_Position = projection * model * vec4(vp, -1.0, 1.0);
	uv = posuv.zw;
}